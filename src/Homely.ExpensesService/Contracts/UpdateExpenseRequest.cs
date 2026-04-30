namespace Homely.ExpensesService.Contracts;

public sealed record UpdateExpenseRequest(
    DateTimeOffset Timestamp,
    decimal Amount,
    string Currency,
    string Type,
    string? Description);
