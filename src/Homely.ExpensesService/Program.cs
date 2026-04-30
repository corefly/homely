using Homely.ExpensesService.Domain;
using Homely.ExpensesService.Endpoints;
using JasperFx;
using Marten;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMarten(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("expensesdb")
        ?? throw new InvalidOperationException("Missing connection string 'expensesdb'.");

    options.Connection(connectionString);
    options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
    options.Schema.For<Expense>()
        .Index(expense => expense.OwnerUserId)
        .Index(expense => expense.Timestamp);
}).UseLightweightSessions();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapExpenseEndpoints();

app.Run();
