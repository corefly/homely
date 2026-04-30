namespace Homely.ExpensesService.Contracts;

public sealed record ExpenseResponse(
    Guid Id,
    DateTimeOffset Timestamp,
    Guid OwnerUserId,
    decimal Amount,
    string Currency,
    string Type,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
