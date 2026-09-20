using FinanceFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Api.Data;

public class FinanceFlowDbContext : DbContext
{
    public FinanceFlowDbContext(
        DbContextOptions<FinanceFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();

    // Configure invoice column requirements, lengths, and amount precision.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(i => i.SupplierName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(i => i.Amount)
                .HasPrecision(18, 2);

            entity.Property(i => i.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(i => i.Status)
                .IsRequired()
                .HasMaxLength(30);
        });
    }
}
