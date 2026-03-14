using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.PaymentMethod;

public sealed record UpdatePaymentMethodRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
