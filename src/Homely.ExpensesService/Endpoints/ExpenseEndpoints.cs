using Homely.ExpensesService.Contracts;
using Homely.ExpensesService.Domain;
using Marten;

namespace Homely.ExpensesService.Endpoints;

public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var expenses = endpoints.MapGroup("/expenses");

        expenses.MapGet("/", ListByOwnerAsync)
            .WithName("ListExpenses");

        expenses.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetExpense");

        expenses.MapPost("/", CreateAsync)
            .WithName("CreateExpense");

        expenses.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateExpense");

        expenses.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteExpense");

        return endpoints;
    }

    private static async Task<IResult> ListByOwnerAsync(
        Guid ownerUserId,
        int? page,
        int? pageSize,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var requestedPage = page ?? 1;
        var requestedPageSize = pageSize ?? 20;

        if (ownerUserId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(ownerUserId)] = ["Owner user id is required."]
            });
        }

        if (requestedPage < 1)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(page)] = ["Page must be greater than zero."]
            });
        }

        if (requestedPageSize is < 1 or > 100)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(pageSize)] = ["Page size must be between 1 and 100."]
            });
        }

        var query = session.Query<Expense>()
            .Where(expense => expense.OwnerUserId == ownerUserId);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)requestedPageSize);

        var expenses = await query
            .OrderByDescending(expense => expense.Timestamp)
            .Skip((requestedPage - 1) * requestedPageSize)
            .Take(requestedPageSize)
            .ToListAsync(cancellationToken);

        return Results.Ok(new PagedResponse<ExpenseResponse>(
            expenses.Select(ToResponse).ToList(),
            requestedPage,
            requestedPageSize,
            totalCount,
            totalPages,
            requestedPage > 1,
            totalPages > 0 && requestedPage < totalPages));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var expense = await session.LoadAsync<Expense>(id, cancellationToken);

        return expense is null ? Results.NotFound() : Results.Ok(ToResponse(expense));
    }

    private static async Task<IResult> CreateAsync(
        CreateExpenseRequest request,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var errors = ExpenseInput.Validate(request);

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var now = DateTimeOffset.UtcNow;
        var expense = new Expense
        {
            Timestamp = request.Timestamp ?? now,
            OwnerUserId = request.OwnerUserId,
            Amount = request.Amount,
            Currency = ExpenseInput.NormalizeCurrency(request.Currency),
            Type = ExpenseInput.NormalizeType(request.Type),
            Description = ExpenseInput.NormalizeDescription(request.Description),
            CreatedAt = now,
            UpdatedAt = now
        };

        session.Store(expense);
        await session.SaveChangesAsync(cancellationToken);

        return Results.Created($"/expenses/{expense.Id}", ToResponse(expense));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateExpenseRequest request,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var errors = ExpenseInput.Validate(request);

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var expense = await session.LoadAsync<Expense>(id, cancellationToken);

        if (expense is null)
        {
            return Results.NotFound();
        }

        expense.Timestamp = request.Timestamp;
        expense.Amount = request.Amount;
        expense.Currency = ExpenseInput.NormalizeCurrency(request.Currency);
        expense.Type = ExpenseInput.NormalizeType(request.Type);
        expense.Description = ExpenseInput.NormalizeDescription(request.Description);
        expense.UpdatedAt = DateTimeOffset.UtcNow;

        session.Store(expense);
        await session.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(expense));
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        session.Delete<Expense>(id);
        await session.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static ExpenseResponse ToResponse(Expense expense)
    {
        return new ExpenseResponse(
            expense.Id,
            expense.Timestamp,
            expense.OwnerUserId,
            expense.Amount,
            expense.Currency,
            expense.Type,
            expense.Description,
            expense.CreatedAt,
            expense.UpdatedAt);
    }
}
