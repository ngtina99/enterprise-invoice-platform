using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceFlow.Api.Data;


public class FinanceFlowDbContextFactory
    : IDesignTimeDbContextFactory<FinanceFlowDbContext>
{
    // Let EF migration tools create the SQLite context without starting the API.
    public FinanceFlowDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FinanceFlowDbContext>()
            .UseSqlite("Data Source=financeflow.db")
            .Options;

        return new FinanceFlowDbContext(options);
    }
}
