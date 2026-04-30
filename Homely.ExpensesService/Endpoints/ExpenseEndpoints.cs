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
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        if (ownerUserId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(ownerUserId)] = ["Owner user id is required."]
            });
        }

        var expenses = await session.Query<Expense>()
            .Where(expense => expense.OwnerUserId == ownerUserId)
            .OrderByDescending(expense => expense.Timestamp)
            .ToListAsync(cancellationToken);

        return Results.Ok(expenses.Select(ToResponse));
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
