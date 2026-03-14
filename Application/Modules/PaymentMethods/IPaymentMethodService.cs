using Backend.Application.Common;
using Backend.Application.Modules.PaymentMethods.Inputs;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;

namespace Backend.Application.Modules.PaymentMethods;

public interface IPaymentMethodService
{
    Task<Result<IReadOnlyList<PaymentMethodModel>>> GetAllPaymentMethodsAsync(CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodModel>> GetPaymentMethodByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodModel>> GetPaymentMethodByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodModel>> CreatePaymentMethodAsync(CreatePaymentMethodInput input, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodModel>> UpdatePaymentMethodAsync(UpdatePaymentMethodInput input, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeletePaymentMethodAsync(int id, CancellationToken cancellationToken = default);
}
