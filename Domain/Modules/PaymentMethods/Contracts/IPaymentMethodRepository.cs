using Backend.Domain.Common.Base;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;

namespace Backend.Domain.Modules.PaymentMethods.Contracts;

public interface IPaymentMethodRepository : IRepositoryBase<PaymentMethodModel, int>
{
    Task<PaymentMethodModel?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> IsInUseAsync(int paymentMethodId, CancellationToken cancellationToken);
}
