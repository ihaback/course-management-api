using Backend.Domain.Modules.PaymentMethods.Contracts;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public sealed class PaymentMethodRepository(CoursesOnlineDbContext context)
    : RepositoryBase<PaymentMethodModel, int, PaymentMethodEntity, CoursesOnlineDbContext>(context), IPaymentMethodRepository
{
    protected override PaymentMethodModel ToModel(PaymentMethodEntity entity)
        => PaymentMethodModel.Reconstitute(entity.Id, entity.Name);

    protected override PaymentMethodEntity ToEntity(PaymentMethodModel paymentMethod)
        => new()
        {
            Id = paymentMethod.Id,
            Name = paymentMethod.Name
        };

    public override async Task<PaymentMethodModel> AddAsync(PaymentMethodModel paymentMethod, CancellationToken cancellationToken)
    {
        var currentMaxId = await _context.PaymentMethods
            .AsNoTracking()
            .MaxAsync(pm => (int?)pm.Id, cancellationToken);

        var entity = new PaymentMethodEntity
        {
            Id = (currentMaxId ?? -1) + 1,
            Name = paymentMethod.Name
        };

        _context.PaymentMethods.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<IReadOnlyList<PaymentMethodModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.PaymentMethods
            .AsNoTracking()
            .OrderByDescending(pm => pm.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<PaymentMethodModel?> GetByIdAsync(int paymentMethodId, CancellationToken cancellationToken)
    {
        if (paymentMethodId < 0)
            throw new ArgumentException("Payment method ID must be zero or positive.", nameof(paymentMethodId));

        var entity = await _context.PaymentMethods
            .AsNoTracking()
            .SingleOrDefaultAsync(pm => pm.Id == paymentMethodId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public async Task<PaymentMethodModel?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentMethods
            .AsNoTracking()
            .SingleOrDefaultAsync(pm => pm.Name == name, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<PaymentMethodModel?> UpdateAsync(int id, PaymentMethodModel paymentMethod, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentMethods
            .SingleOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Payment method '{paymentMethod.Id}' not found.");

        entity.Name = paymentMethod.Name;
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<bool> RemoveAsync(int paymentMethodId, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentMethods
            .SingleOrDefaultAsync(pm => pm.Id == paymentMethodId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Payment method '{paymentMethodId}' not found.");

        _context.PaymentMethods.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsInUseAsync(int paymentMethodId, CancellationToken cancellationToken)
    {
        if (paymentMethodId < 0)
            throw new ArgumentException("Payment method ID must be zero or positive.", nameof(paymentMethodId));

        return await _context.CourseRegistrations
            .AsNoTracking()
            .AnyAsync(cr => cr.PaymentMethodId == paymentMethodId, cancellationToken);
    }
}
