namespace Homely.ExpensesService.Domain;

public sealed class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset Timestamp { get; set; }

    public Guid OwnerUserId { get; set; }

    public decimal Amount { get; set; }

    public required string Currency { get; set; }

    public required string Type { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
