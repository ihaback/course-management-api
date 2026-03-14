using Backend.Application.Common;
using Backend.Application.Modules.PaymentMethods.Caching;
using Backend.Application.Modules.PaymentMethods.Inputs;

using Backend.Domain.Modules.PaymentMethods.Contracts;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;

namespace Backend.Application.Modules.PaymentMethods;

public sealed class PaymentMethodService(IPaymentMethodCache cache, IPaymentMethodRepository repository) : IPaymentMethodService
{
    private readonly IPaymentMethodCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IPaymentMethodRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<Result<PaymentMethodModel>> CreatePaymentMethodAsync(CreatePaymentMethodInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
                return Result<PaymentMethodModel>.BadRequest("Payment method cannot be null.");

            var existing = await _repository.GetByNameAsync(input.Name, cancellationToken);
            if (existing is not null)
                return Result<PaymentMethodModel>.BadRequest("A payment method with the same name already exists.");

            var created = await _repository.AddAsync(PaymentMethodModel.Create(input.Name), cancellationToken);
            _cache.ResetEntity(created);
            _cache.SetEntity(created);
            return Result<PaymentMethodModel>.Ok(created);
        }
        catch (ArgumentException ex)
        {
            return Result<PaymentMethodModel>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<PaymentMethodModel>.Error("An error occurred while creating the payment method.");
        }
    }

    public async Task<Result<IReadOnlyList<PaymentMethodModel>>> GetAllPaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentMethods = await _cache.GetAllAsync(
                token => _repository.GetAllAsync(token),
                cancellationToken);
            return Result<IReadOnlyList<PaymentMethodModel>>.Ok(paymentMethods);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<PaymentMethodModel>>.Error("An error occurred while retrieving payment methods.");
        }
    }

    public async Task<Result<PaymentMethodModel>> GetPaymentMethodByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return Result<PaymentMethodModel>.BadRequest("Id must be greater than zero.");

            var paymentMethod = await _cache.GetByIdAsync(
                id,
                token => _repository.GetByIdAsync(id, token),
                cancellationToken);
            if (paymentMethod == null)
                return Result<PaymentMethodModel>.NotFound($"Payment method with ID '{id}' not found.");

            return Result<PaymentMethodModel>.Ok(paymentMethod);
        }
        catch (ArgumentException ex)
        {
            return Result<PaymentMethodModel>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<PaymentMethodModel>.Error("An error occurred while retrieving the payment method.");
        }
    }

    public async Task<Result<PaymentMethodModel>> GetPaymentMethodByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<PaymentMethodModel>.BadRequest("Name is required.");

            var paymentMethod = await _cache.GetByNameAsync(
                name,
                token => _repository.GetByNameAsync(name, token),
                cancellationToken);
            if (paymentMethod == null)
                return Result<PaymentMethodModel>.NotFound($"Payment method with name '{name}' not found.");

            return Result<PaymentMethodModel>.Ok(paymentMethod);
        }
        catch (ArgumentException ex)
        {
            return Result<PaymentMethodModel>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<PaymentMethodModel>.Error("An error occurred while retrieving the payment method.");
        }
    }

    public async Task<Result<PaymentMethodModel>> UpdatePaymentMethodAsync(UpdatePaymentMethodInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
                return Result<PaymentMethodModel>.BadRequest("Payment method cannot be null.");

            var existingPaymentMethod = await _repository.GetByIdAsync(input.Id, cancellationToken);
            if (existingPaymentMethod == null)
                return Result<PaymentMethodModel>.NotFound($"Payment method with ID '{input.Id}' not found.");

            existingPaymentMethod.Update(input.Name);
            var updatedPaymentMethod = await _repository.UpdateAsync(existingPaymentMethod.Id, existingPaymentMethod, cancellationToken);
            if (updatedPaymentMethod == null)
                return Result<PaymentMethodModel>.Error("Failed to update payment method.");
            _cache.ResetEntity(existingPaymentMethod);
            _cache.SetEntity(updatedPaymentMethod);

            return Result<PaymentMethodModel>.Ok(updatedPaymentMethod);
        }
        catch (ArgumentException ex)
        {
            return Result<PaymentMethodModel>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<PaymentMethodModel>.Error("An error occurred while updating the payment method.");
        }
    }

    public async Task<Result<bool>> DeletePaymentMethodAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return Result<bool>.BadRequest("Id must be greater than zero.");

            var existingPaymentMethod = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingPaymentMethod == null)
                return Result<bool>.NotFound($"Payment method with ID '{id}' not found.");

            if (await _repository.IsInUseAsync(id, cancellationToken))
                return Result<bool>.Conflict($"Cannot delete payment method with ID '{id}' because it is in use.");

            var isDeleted = await _repository.RemoveAsync(id, cancellationToken);
            if (!isDeleted)
                return Result<bool>.Error("Failed to delete payment method.");

            _cache.ResetEntity(existingPaymentMethod);
            return Result<bool>.Ok(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the payment method.");
        }
    }
}

