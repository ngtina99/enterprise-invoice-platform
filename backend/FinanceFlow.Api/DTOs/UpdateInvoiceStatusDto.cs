using System.ComponentModel.DataAnnotations;

namespace FinanceFlow.Api.DTOs;

// Require a status value; the controller checks and normalizes supported values.
public class UpdateInvoiceStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;
}
