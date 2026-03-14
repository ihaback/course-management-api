using Backend.Application.Common.Caching;
using Microsoft.Extensions.Caching.Memory;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;

namespace Backend.Application.Modules.PaymentMethods.Caching;

public sealed class PaymentMethodCache(IMemoryCache cache) : CacheEntityBase<PaymentMethodModel, int>(cache), IPaymentMethodCache
{
    protected override int GetId(PaymentMethodModel entity) => entity.Id;

    protected override IEnumerable<(string PropertyName, string Value)> GetCachedProperties(PaymentMethodModel entity)
        => [("name", entity.Name)];

    public Task<PaymentMethodModel?> GetByIdAsync(int id, Func<CancellationToken, Task<PaymentMethodModel?>> factory, CancellationToken ct)
        => GetOrCreateByIdAsync(id, factory, ct);

    public Task<PaymentMethodModel?> GetByNameAsync(string name, Func<CancellationToken, Task<PaymentMethodModel?>> factory, CancellationToken ct)
        => GetOrCreateByPropertyNameAsync("name", name, factory, ct);

    public Task<IReadOnlyList<PaymentMethodModel>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<PaymentMethodModel>>> factory, CancellationToken ct)
        => GetOrCreateAllAsync(factory, ct);
}
