using Homely.ExpensesService.Contracts;

namespace Homely.ExpensesService.Endpoints;

internal static class ExpenseInput
{
    public static IDictionary<string, string[]> Validate(CreateExpenseRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.OwnerUserId == Guid.Empty)
        {
            errors[nameof(request.OwnerUserId)] = ["Owner user id is required."];
        }

        ValidateAmountCurrencyAndType(request.Amount, request.Currency, request.Type, errors);

        return errors;
    }

    public static IDictionary<string, string[]> Validate(UpdateExpenseRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateAmountCurrencyAndType(request.Amount, request.Currency, request.Type, errors);

        return errors;
    }

    public static string NormalizeCurrency(string currency)
    {
        return currency.Trim().ToUpperInvariant();
    }

    public static string NormalizeType(string type)
    {
        return type.Trim().ToLowerInvariant();
    }

    public static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    private static void ValidateAmountCurrencyAndType(
        decimal amount,
        string? currency,
        string? type,
        Dictionary<string, string[]> errors)
    {
        if (amount <= 0)
        {
            errors[nameof(amount)] = ["Amount must be greater than zero."];
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            errors[nameof(currency)] = ["Currency is required."];
        }
        else if (currency.Trim().Length != 3)
        {
            errors[nameof(currency)] = ["Currency must be a 3-letter ISO code."];
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            errors[nameof(type)] = ["Expense type is required."];
        }
    }
}
