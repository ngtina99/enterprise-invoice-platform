using FinanceFlow.Api.Models;

// Imports Entity Framework Core types such as DbContext, DbSet, DbContextOptions, and ModelBuilder.
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Api.Data;

// Database context class.
// ": DbContext" inherits from EF Core's DbContext class.
public class FinanceFlowDbContext : DbContext
{
    // Constructor: receives the database configuration through dependency injection when ASP.NET Core creates this DbContext.
    public FinanceFlowDbContext(
        DbContextOptions<FinanceFlowDbContext> options)

        // Passes the received options to the parent DbContext constructor.
        : base(options)
    {
        // Nothing else needs to happen in this constructor.
    }

    // Provides access to Invoice entities in the database.
    // We can use it to query, add, update, and remove invoices.
    //
    // "=>" is an expression-bodied property: accessing Invoices returns the result of Set<Invoice>().
    //
    // Set<Invoice>() gets the DbSet for the Invoice entity.
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // EF Core calls this method when building its model.
    // We override it to configure how Invoice maps to the database.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the Invoice entity.
        // "entity" represents a builder used to configure Invoice.
        modelBuilder.Entity<Invoice>(entity =>
        {
            // Sets Id as the primary key. A primary key uniquely identifies each invoice.
            entity.HasKey(i => i.Id);

            // Configure the InvoiceNumber property.
            entity.Property(i => i.InvoiceNumber)
                .IsRequired()      // The database column cannot be NULL.
                .HasMaxLength(50);  // Maximum length: 50 characters.

            // Configure the SupplierName property.
            entity.Property(i => i.SupplierName)
                .IsRequired()       // The database column cannot be NULL.
                .HasMaxLength(200);  // Maximum length: 200 characters.

            // Configure the Amount property.
            entity.Property(i => i.Amount)
                .HasPrecision(18, 2); // 18 total digits, including
                                      // 2 digits after the decimal point.

            // Configure the Currency property.
            entity.Property(i => i.Currency)
                .IsRequired()     // The database column cannot be NULL.
                .HasMaxLength(3);  // Example: "HUF", "EUR", "USD".

            // Configure the Status property.
            entity.Property(i => i.Status)
                .IsRequired()      // The database column cannot be NULL.
                .HasMaxLength(30);  // Example: "Draft" or "Paid".
        });
    }
}