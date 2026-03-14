namespace Backend.Infrastructure.Persistence.Entities;

public class PaymentMethodEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public byte[] Concurrency { get; set; } = null!;
}
