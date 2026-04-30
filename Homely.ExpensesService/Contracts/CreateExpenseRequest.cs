namespace Homely.ExpensesService.Contracts;

public sealed record CreateExpenseRequest(
    DateTimeOffset? Timestamp,
    Guid OwnerUserId,
    decimal Amount,
    string Currency,
    string Type,
    string? Description);
