namespace Homely.ExpensesService.Contracts;

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
