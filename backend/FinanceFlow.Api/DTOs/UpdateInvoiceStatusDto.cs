using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Api.DTOs;

// Defines the data the client sends when updating an invoice status.
public class UpdateInvoiceStatusDto
{
    // Required: the client must provide a status.
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;
}