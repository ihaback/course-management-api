using Backend.Application.Common.Caching;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;

namespace Backend.Application.Modules.PaymentMethods.Caching;

public interface IPaymentMethodCache : ICacheEntityBase<PaymentMethodModel, int>
{
    Task<IReadOnlyList<PaymentMethodModel>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<PaymentMethodModel>>> factory, CancellationToken ct);
    Task<PaymentMethodModel?> GetByIdAsync(int id, Func<CancellationToken, Task<PaymentMethodModel?>> factory, CancellationToken ct);
    Task<PaymentMethodModel?> GetByNameAsync(string name, Func<CancellationToken, Task<PaymentMethodModel?>> factory, CancellationToken ct);
}
