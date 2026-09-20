using FinanceFlow.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Persist invoices in the local SQLite database.
builder.Services.AddDbContext<FinanceFlowDbContext>(options =>
    options.UseSqlite("Data Source=financeflow.db"));
builder.Services.AddOpenApi();

var app = builder.Build();
// Expose the OpenAPI document only during development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseAuthorization();
app.MapControllers();

app.Run();
