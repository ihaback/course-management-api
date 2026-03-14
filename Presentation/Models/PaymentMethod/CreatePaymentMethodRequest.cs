using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.PaymentMethod;

public sealed record CreatePaymentMethodRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
