using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseEventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEventTypes_Id", x => x.Id);
                    table.CheckConstraint("CK_CourseEventTypes_TypeName_NotEmpty", "LTRIM(RTRIM([Name])) <> ''");
                });

            migrationBuilder.CreateTable(
                name: "CourseRegistrationStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRegistrationStatuses_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(NEWSEQUENTIALID())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Courses_Id"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DurationInDays = table.Column<int>(type: "int", nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Courses_CreatedAtUtc"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Courses_ModifiedAtUtc")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses_Id", x => x.Id);
                    table.CheckConstraint("CK_Courses_Title_NotEmpty", "LTRIM(RTRIM([Title])) <> ''");
                });

            migrationBuilder.CreateTable(
                name: "InstructorRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorRoles_Id", x => x.Id);
                    table.CheckConstraint("CK_InstructorRoles_RoleName_NotEmpty", "LEN([Name]) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreetName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations_Id", x => x.Id);
                    table.CheckConstraint("CK_Locations_PostalCode_NotEmpty", "LTRIM(RTRIM([PostalCode])) <> ''");
                });

            migrationBuilder.CreateTable(
                name: "ParticipantContactTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantContactTypes_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VenueTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueTypes_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(NEWSEQUENTIALID())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Instructors_Id"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InstructorRoleId = table.Column<int>(type: "int", nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructors_Id", x => x.Id);
                    table.CheckConstraint("CK_Instructors_Name_NotEmpty", "LTRIM(RTRIM([Name])) <> ''");
                    table.ForeignKey(
                        name: "FK_Instructors_InstructorRoles_InstructorRoleId",
                        column: x => x.InstructorRoleId,
                        principalTable: "InstructorRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InPlaceLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    RoomNumber = table.Column<int>(type: "int", nullable: false),
                    Seats = table.Column<int>(type: "int", nullable: false),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InPlaceLocations_Id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InPlaceLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(NEWSEQUENTIALID())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Participants_Id"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContactTypeId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Participants_CreatedAtUtc"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Participants_ModifiedAtUtc")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants_Id", x => x.Id);
                    table.CheckConstraint("CK_Participants_Email_NotEmpty", "LTRIM(RTRIM([Email])) <> ''");
                    table.ForeignKey(
                        name: "FK_Participants_ParticipantContactTypes_ContactTypeId",
                        column: x => x.ContactTypeId,
                        principalTable: "ParticipantContactTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(NEWSEQUENTIALID())")
                        .Annotation("Relational:DefaultConstraintName", "DF_CourseEvents_Id"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Price = table.Column<decimal>(type: "money", nullable: false),
                    Seats = table.Column<int>(type: "int", nullable: false),
                    CourseEventTypeId = table.Column<int>(type: "int", nullable: false),
                    VenueTypeId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                        .Annotation("Relational:DefaultConstraintName", "DF_CourseEvents_CreatedAtUtc"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                        .Annotation("Relational:DefaultConstraintName", "DF_CourseEvents_ModifiedAtUtc")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEvents_Id", x => x.Id);
                    table.CheckConstraint("CK_CourseEvents_Price", "[Price] >= 0");
                    table.CheckConstraint("CK_CourseEvents_Seats", "[Seats] > 0");
                    table.ForeignKey(
                        name: "FK_CourseEvents_CourseEventTypes_CourseEventTypeId",
                        column: x => x.CourseEventTypeId,
                        principalTable: "CourseEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseEvents_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseEvents_VenueTypes_VenueTypeId",
                        column: x => x.VenueTypeId,
                        principalTable: "VenueTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseEventInstructors",
                columns: table => new
                {
                    CourseEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEventInstructors", x => new { x.CourseEventId, x.InstructorId });
                    table.ForeignKey(
                        name: "FK_CourseEventInstructors_CourseEvents_CourseEventId",
                        column: x => x.CourseEventId,
                        principalTable: "CourseEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseEventInstructors_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(NEWSEQUENTIALID())")
                        .Annotation("Relational:DefaultConstraintName", "DF_CourseRegistrations_Id"),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                        .Annotation("Relational:DefaultConstraintName", "DF_CourseRegistrations_RegistrationDate"),
                    CourseRegistrationStatusId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Concurrency = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                        .Annotation("Relational:DefaultConstraintName", "DF_CourseRegistrations_ModifiedAtUtc")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRegistrations_Id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseRegistrations_CourseEvents_CourseEventId",
                        column: x => x.CourseEventId,
                        principalTable: "CourseEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseRegistrations_CourseRegistrationStatuses_CourseRegistrationStatusId",
                        column: x => x.CourseRegistrationStatusId,
                        principalTable: "CourseRegistrationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseRegistrations_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseRegistrations_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InPlaceEventLocations",
                columns: table => new
                {
                    CourseEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InPlaceLocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InPlaceEventLocations", x => new { x.CourseEventId, x.InPlaceLocationId });
                    table.ForeignKey(
                        name: "FK_InPlaceEventLocations_CourseEvents_CourseEventId",
                        column: x => x.CourseEventId,
                        principalTable: "CourseEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InPlaceEventLocations_InPlaceLocations_InPlaceLocationId",
                        column: x => x.InPlaceLocationId,
                        principalTable: "InPlaceLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseEventInstructors_InstructorId",
                table: "CourseEventInstructors",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEvents_CourseEventTypeId",
                table: "CourseEvents",
                column: "CourseEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEvents_CourseId_EventDate",
                table: "CourseEvents",
                columns: new[] { "CourseId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseEvents_VenueTypeId",
                table: "CourseEvents",
                column: "VenueTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEventTypes_Name",
                table: "CourseEventTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrations_CourseEventId",
                table: "CourseRegistrations",
                column: "CourseEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrations_CourseRegistrationStatusId",
                table: "CourseRegistrations",
                column: "CourseRegistrationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrations_ParticipantId_CourseEventId",
                table: "CourseRegistrations",
                columns: new[] { "ParticipantId", "CourseEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrations_PaymentMethodId",
                table: "CourseRegistrations",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrationStatuses_Name",
                table: "CourseRegistrationStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Title",
                table: "Courses",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_InPlaceEventLocations_InPlaceLocationId",
                table: "InPlaceEventLocations",
                column: "InPlaceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InPlaceLocations_LocationId_RoomNumber",
                table: "InPlaceLocations",
                columns: new[] { "LocationId", "RoomNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorRoles_Name",
                table: "InstructorRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_InstructorRoleId",
                table: "Instructors",
                column: "InstructorRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_Name",
                table: "Instructors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_PostalCode",
                table: "Locations",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantContactTypes_Name",
                table: "ParticipantContactTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ContactTypeId",
                table: "Participants",
                column: "ContactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_Email",
                table: "Participants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Name",
                table: "PaymentMethods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VenueTypes_Name",
                table: "VenueTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseEventInstructors");

            migrationBuilder.DropTable(
                name: "CourseRegistrations");

            migrationBuilder.DropTable(
                name: "InPlaceEventLocations");

            migrationBuilder.DropTable(
                name: "Instructors");

            migrationBuilder.DropTable(
                name: "CourseRegistrationStatuses");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "CourseEvents");

            migrationBuilder.DropTable(
                name: "InPlaceLocations");

            migrationBuilder.DropTable(
                name: "InstructorRoles");

            migrationBuilder.DropTable(
                name: "ParticipantContactTypes");

            migrationBuilder.DropTable(
                name: "CourseEventTypes");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "VenueTypes");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}

