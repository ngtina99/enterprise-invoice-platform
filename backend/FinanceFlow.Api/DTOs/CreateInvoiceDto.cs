using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Api.DTOs;

// Validate invoice creation input; the server assigns status and creation time.
public class CreateInvoiceDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string SupplierName { get; set; } = string.Empty;

    [Range(
        typeof(decimal),
        "0.01",
        "999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Amount must be greater than zero."
    )]
    public decimal Amount { get; set; }

    [Required]
    [RegularExpression("^[A-Z]{3}$")]
    public string Currency { get; set; } = "HUF";
}
