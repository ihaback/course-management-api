using Backend.Application.Common;
using Backend.Application.Modules.PaymentMethods;
using Backend.Application.Modules.PaymentMethods.Caching;
using Backend.Application.Modules.PaymentMethods.Inputs;
using Backend.Domain.Modules.PaymentMethods.Contracts;
using NSubstitute;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;

namespace Backend.Tests.Unit.Application.Modules.PaymentMethods;

public class PaymentMethodService_Tests
{
    [Fact]
    public async Task GetAll_Should_Return_Items()
    {
        var repo = Substitute.For<IPaymentMethodRepository>();
        var cache = Substitute.For<IPaymentMethodCache>();
        cache.GetAllAsync(Arg.Any<Func<CancellationToken, Task<IReadOnlyList<PaymentMethodModel>>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<IReadOnlyList<PaymentMethodModel>>>>()(ci.Arg<CancellationToken>()));
        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([PaymentMethodModel.Reconstitute(1, "Card"), PaymentMethodModel.Reconstitute(2, "Invoice")]);
        var service = new PaymentMethodService(cache, repo);

        var result = await service.GetAllPaymentMethodsAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetById_Should_Return_From_Cache_Without_Repo_Call()
    {
        var repo = Substitute.For<IPaymentMethodRepository>();
        var cache = Substitute.For<IPaymentMethodCache>();
        var cached = PaymentMethodModel.Reconstitute(5, "Swish");
        cache.GetByIdAsync(5, Arg.Any<Func<CancellationToken, Task<PaymentMethodModel?>>>(), Arg.Any<CancellationToken>())
            .Returns(cached);
        var service = new PaymentMethodService(cache, repo);

        var result = await service.GetPaymentMethodByIdAsync(5, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(cached, result.Value);
        await repo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_Should_Reset_And_Set_Cache()
    {
        var repo = Substitute.For<IPaymentMethodRepository>();
        var cache = Substitute.For<IPaymentMethodCache>();
        var existing = PaymentMethodModel.Reconstitute(2, "Card");
        var updated = PaymentMethodModel.Reconstitute(2, "Invoice");

        repo.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        repo.UpdateAsync(existing.Id, Arg.Any<PaymentMethodModel>(), Arg.Any<CancellationToken>())
            .Returns(updated);

        var service = new PaymentMethodService(cache, repo);

        var result = await service.UpdatePaymentMethodAsync(new UpdatePaymentMethodInput(existing.Id, "Invoice"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(updated, result.Value);
        cache.Received(1).ResetEntity(existing);
        cache.Received(1).SetEntity(updated);
    }
}

