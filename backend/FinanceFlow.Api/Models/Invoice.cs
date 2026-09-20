namespace FinanceFlow.Api.Models;

public class Invoice
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "HUF";

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}