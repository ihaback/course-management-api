# Course Management System — Backend

A .NET REST API for managing online courses, events, instructors, participants, and registrations. Demonstrates practical application of **Domain-Driven Design**, **Clean Architecture**, and production-grade testing practices.

## Table of Contents

- [Architecture](#architecture)
- [DDD Patterns](#ddd-patterns)
- [Domain Model](#domain-model)
- [API Endpoints](#api-endpoints)
- [Database Strategy](#database-strategy)
- [Caching](#caching)
- [Testing](#testing)
- [Getting Started](#getting-started)
- [Technology Stack](#technology-stack)

---

## Architecture

The solution is structured as four projects following Clean Architecture. Dependencies only point inward — infrastructure and presentation know about application and domain, but domain knows nothing about the others.

```
Presentation → Application → Domain
Infrastructure → Application → Domain
```

| Project | Responsibility |
|---------|---------------|
| `Domain` | Business models, validation rules, Value Objects, repository contracts |
| `Application` | Services, DTOs (Input/Output), caching for lookup entities |
| `Infrastructure` | EF Core `DbContext`, repositories, entity configs, migrations |
| `Presentation` | Minimal API endpoints, request models, HTTP result mapping |

### Folder structure per module

```
Domain/Modules/{Module}/Models/
Domain/Modules/{Module}/Contracts/I{Entity}Repository.cs
Domain/Common/ValueObjects/           ← shared Value Objects

Application/Modules/{Module}/I{Entity}Service.cs
Application/Modules/{Module}/{Entity}Service.cs
Application/Modules/{Module}/Inputs/
Application/Modules/{Module}/Outputs/
Application/Modules/{Module}/Caching/ ← lookup entities only

Infrastructure/Persistence/EFC/Repositories/
Infrastructure/Persistence/EFC/Configurations/
Infrastructure/Persistence/Entities/
Infrastructure/Persistence/           ← DatabaseSeeder, PersistenceDatabaseInitializer

Presentation/Endpoints/
Presentation/Models/{Module}/

Tests/Unit/Domain/
Tests/Unit/Application/
Tests/Integration/Infrastructure/
Tests/E2E/
```

---

## DDD Patterns

### Value Objects

Three Value Objects live in `Domain/Common/ValueObjects/` and are used as property types on domain models:

| Value Object | Used on | Validation |
|---|---|---|
| `Email` | `Participant.Email` | Regex format check, not null/whitespace |
| `PhoneNumber` | `Participant.PhoneNumber` | Not null/whitespace |
| `Price` | `CourseEvent.Price` | Non-negative decimal |

Each VO is:
- **Sealed and immutable** — `Value` is the only public member
- **Created via static factory** — `Email.Create(value, paramName)` throws `ArgumentException` on invalid input
- **Structurally equal** — `IEquatable<T>` + `==`/`!=` operator overloads
- **JSON-serializable** — inline `JsonConverter` ensures round-trip through System.Text.Json without manual configuration

```csharp
// Domain model using VOs
public Email Email { get; private set; }
public PhoneNumber PhoneNumber { get; private set; }

// Repository extracting primitives for EF
entity.Email = participant.Email.Value;
```

### Constructor + Update + SetValues Pattern

All domain models share a single validation path. Constructors and `Update()` both delegate to a private `SetValues()` method, ensuring invariants are enforced regardless of how the model is created or mutated.

```csharp
// [JsonConstructor] — used by deserializer, accepts VO types
private Participant(Guid id, string firstName, ..., Email email, PhoneNumber phoneNumber, ...)
    => SetValues(firstName, ..., email, phoneNumber, ...);

// Factory — takes primitives, creates VOs, calls constructor
public static Participant Create(string firstName, ..., string email, string phoneNumber, ...)
    => new(Guid.NewGuid(), firstName, ..., Email.Create(email, nameof(email)), ...);

// Reconstitute — hydrates from DB, same shape as Create but with existing ID
public static Participant Reconstitute(Guid id, string firstName, ..., string email, ...) => ...;

// Update — mutates existing instance via the same SetValues path
public void Update(string firstName, ..., string email, string phoneNumber, ...)
    => SetValues(firstName, ..., Email.Create(email, nameof(email)), ...);
```

- `Reconstitute(...)` is used exclusively by repositories — it bypasses side effects a fresh `Create` might trigger (e.g. raising domain events in future)
- Service update flow: `existing = await repo.GetAsync(id)` → `existing.Update(...)` → `await repo.UpdateAsync(existing)`

### Result Pattern

All service methods return `ResultBase` or `ResultBase<T>` instead of throwing exceptions for control flow. Endpoints call `.ToHttpResult()` which maps to the correct HTTP status code.

```csharp
// Service
if (existing is null) return ResultBase.NotFound("Course not found.");
return ResultBase.Ok(MapToResult(existing));

// Endpoint
var result = await service.GetCourseAsync(id);
return result.ToHttpResult();  // → 200 OK or 404 Not Found
```

Available result states: `Ok`, `NotFound`, `BadRequest`, `Conflict`, `Unprocessable`.

### Repository Pattern

`RepositoryBase<TModel, TId, TEntity, TDbContext>` provides default CRUD operations. Each concrete repository implements:

- `ToEntity(model)` — maps domain model → EF entity (extracts `.Value` from VOs)
- `ToModel(entity)` — maps EF entity → domain model via `Reconstitute(...)` (includes required lookups via `Include(...)`, fails fast if missing)

Transactions are used explicitly in repositories with multi-step atomic operations (`CourseRegistrationRepository`, `CourseEventRepository`, `ParticipantRepository`). Raw SQL (`Database.SqlQuery<T>` / `Database.ExecuteSqlAsync`) is used only for seat-availability checks and relation-table cleanup in those same repositories.

---

## Domain Model

| Entity | Key Properties |
|--------|---------------|
| `Course` | `Title`, `Description`, `DurationInDays` |
| `CourseEvent` | `CourseId`, `EventDate`, `Price` *(VO)*, `Seats`, `CourseEventTypeId`, `VenueTypeId` |
| `CourseEventType` | `Name` *(lookup)* |
| `CourseRegistration` | `CourseEventId`, `ParticipantId`, `StatusId`, `PaymentMethodId`, `RegisteredAt` |
| `CourseRegistrationStatus` | `Name` *(lookup)* |
| `Participant` | `FirstName`, `LastName`, `Email` *(VO)*, `PhoneNumber` *(VO)*, `ContactTypeId` |
| `ParticipantContactType` | `Name` *(lookup)* |
| `Instructor` | `Name`, `RoleId` |
| `InstructorRole` | `Name` *(lookup)* |
| `Location` | `StreetName`, `PostalCode`, `City` |
| `InPlaceLocation` | `LocationId`, `RoomName`, `Capacity` |
| `PaymentMethod` | `Name` *(lookup)* |
| `VenueType` | `Name` *(lookup)* |

---

## API Endpoints

All endpoints are prefixed with `/api`. OpenAPI spec available at `/openapi/v1.json`.

| Resource | Endpoints |
|----------|-----------|
| Courses | `GET /api/courses` · `GET /api/courses/{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| Course Events | `GET /api/course-events` · `GET /{id}` · `GET /course/{courseId}` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| Course Event Types | Full CRUD |
| Course Registrations | `GET` (all/by id/by participant/by event) · `POST` · `PUT /{id}` · `DELETE /{id}` |
| Course Registration Statuses | Full CRUD |
| Participants | Full CRUD |
| Participant Contact Types | Full CRUD |
| Instructors | Full CRUD |
| Instructor Roles | Full CRUD |
| Locations | Full CRUD |
| In-Place Locations | `GET` (all/by id/by location) · `POST` · `PUT /{id}` · `DELETE /{id}` |
| Payment Methods | `GET` (all/by id/by name) · `POST` · `PUT /{id}` · `DELETE /{id}` |
| Venue Types | `GET` (all/by id/by name) · `POST` · `PUT /{id}` · `DELETE /{id}` |

---

## Database Strategy

### Provider switching

The infrastructure layer switches database provider based on environment:

| Environment | Provider | Schema init |
|---|---|---|
| Development | SQLite in-memory (shared cache) | `EnsureCreatedAsync` |
| Production | SQL Server | `MigrateAsync` |
| Tests | SQLite in-memory (via `DB_PROVIDER=Sqlite`) | `EnsureCreatedAsync` |

`ContextRegistrationExtension` checks `env.IsDevelopment()` to register the correct provider. A singleton `SqliteConnection` is kept open for the lifetime of the dev app so the in-memory database survives across requests.

### Seeding

`DatabaseSeeder` seeds reference data in both dev and production on startup, in dependency order:

1. Fixed-ID lookup tables (VenueTypes, PaymentMethods, etc.)
2. Auto-ID lookup tables
3. Transactional data (Courses, Events, Participants, Registrations)

Seeding is idempotent — guarded by `AnyAsync()` checks before inserting. It runs via `PersistenceDatabaseInitializer.InitializeAsync` called from `Program.cs` after `app.Build()`.

### Migrations

SQL Server-specific migrations live in `Infrastructure/Persistence/EFC/Migrations/`. They are only applied in production. Dev and test environments use `EnsureCreatedAsync` — no migrations needed.

### EF Entity Configuration

Configurations take a `bool isSqlite` constructor parameter to handle the concurrency token difference between providers:

```csharp
if (isSqlite)
    e.Property(x => x.Concurrency).IsConcurrencyToken().IsRequired(false);
else
    e.Property(x => x.Concurrency).IsRowVersion().IsConcurrencyToken().IsRequired();
```

---

## Caching

Caching lives in the **Application layer** — never in repositories. Only lookup/reference entities are cached (entities that are read frequently and rarely change).

- Base abstractions: `CacheEntityBase`, `ICacheEntityBase<TEntity, TId>`, `MemoryCacheExtensions`
- Concrete caches: `CourseEventTypeCache`, `CourseRegistrationStatusCache`, `InstructorRoleCache`, `PaymentMethodCache`, `VenueTypeCache`, `ParticipantContactTypeCache`
- Cache is invalidated on any write operation

---

## Testing

**All tests passing.**

| Type | Coverage | Technology |
|------|-------|-----------|
| Unit | Domain models, services, Value Objects, input DTOs | NSubstitute mocks, no DB |
| Integration | Every repository | SQLite in-memory via `SqliteInMemoryFixture` |
| E2E | Every endpoint | `WebApplicationFactory` + SQLite in-memory, HTTP round-trips |

### Unit tests

- Domain model tests: constructor, `Update(...)`, `Reconstitute(...)`, all validation paths
- Value Object tests: `Email`, `PhoneNumber`, `Price` — valid/invalid inputs, equality, operators
- Application service tests: all CRUD flows, result states, NSubstitute mocks for repositories
- Input DTO validation tests

### Integration tests

- Real EF Core DbContext against SQLite in-memory
- Every repository tested: CRUD, ordering, filtering, constraint violations
- `SqliteInMemoryFixture` sets `DB_PROVIDER=Sqlite`; infrastructure detects this and uses SQLite automatically

### E2E tests

- Full HTTP request/response via `WebApplicationFactory`
- Data is deleted and reseeded between tests via raw SQL for isolation
- Covers every endpoint: status codes, response shape, ordering, error responses

```bash
# Run all
dotnet test

# By type
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Tests.Unit"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Tests.Integration"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Tests.E2E"

# Single test class
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~CourseEvent_Tests"
```

---

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- SQL Server (for production profile)

### Run in development (SQLite, no SQL Server needed)

```bash
dotnet restore
dotnet dev-certs https --trust
dotnet run --project Presentation
```

The dev profile uses SQLite in-memory and seeds data automatically on startup.

- API: `https://localhost:7118` / `http://localhost:5400`
- OpenAPI: `https://localhost:7118/openapi/v1.json`

### Run in production (SQL Server)

1. Set the connection string in `Presentation/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "CoursesOnlineDatabase": "Data Source=localhost;Initial Catalog=CoursesOnline;..."
     }
   }
   ```

2. Apply migrations:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project Presentation
   ```

3. Run with the production profile:
   ```bash
   dotnet run --project Presentation --launch-profile https-prod
   ```

---

## Technology Stack

| Concern | Technology |
|---------|-----------|
| Framework | ASP.NET Core — Minimal API |
| ORM | Entity Framework Core |
| Database (prod) | SQL Server |
| Database (dev/test) | SQLite in-memory |
| Testing | xUnit, NSubstitute, `WebApplicationFactory` |
| API docs | OpenAPI (built-in ASP.NET Core) |
| Nullable reference types | Enabled project-wide |

