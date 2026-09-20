using FinanceFlow.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services.AddControllers();

// Register the database context in dependency injection.
// SQLite stores the data in a local file named financeflow.db.
// No Azure account, SQL Server, username, or password is required.
builder.Services.AddDbContext<FinanceFlowDbContext>(options =>
    options.UseSqlite("Data Source=financeflow.db"));

// Register OpenAPI services.
builder.Services.AddOpenApi();

var app = builder.Build();

// Enable the OpenAPI document in development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Configure the HTTP request pipeline.
// app.UseHttpsRedirection();
app.UseAuthorization();

// Map controller endpoints, such as /api/invoices.
app.MapControllers();

app.Run();