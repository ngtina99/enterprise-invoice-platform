using FinanceFlow.Api.DTOs;
using FinanceFlow.Api.Models;
using FinanceFlow.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace FinanceFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly FinanceFlowDbContext _context;

    public InvoicesController(FinanceFlowDbContext context)
    {
        _context = context;
    }

    // Return all saved invoices.
    [HttpGet]
    public async Task<ActionResult<List<Invoice>>> GetAll()
    {
        var invoices = await _context.Invoices.ToListAsync();
        return Ok(invoices);
    }

    // Return a single invoice, or 404 when it does not exist.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Invoice>> GetById(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }
        return Ok(invoice);
    }

    // Create an invoice and return its resource URL with the saved data.
    [HttpPost]
    public async Task<ActionResult<Invoice>> Create(
        CreateInvoiceDto request)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = request.InvoiceNumber,
            SupplierName = request.SupplierName,
            Amount = request.Amount,
            Currency = request.Currency,
            // New invoices always start as Pending, regardless of client input.
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            invoice
        );
    }

    // Update an existing invoice to one of the supported statuses.
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<Invoice>> UpdateStatus(
        int id,
        UpdateInvoiceStatusDto request)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice is null)
        {
            return NotFound(new
            {
                message = $"Invoice with ID {id} was not found."
            });
        }
        // Accept surrounding whitespace and any casing, then store the canonical status.
        var status = request.Status?.Trim();
        var supportedStatuses = new[] { "Pending", "Approved", "Rejected" };
        var canonicalStatus = Array.Find(supportedStatuses,
            candidate => string.Equals(status, candidate, StringComparison.OrdinalIgnoreCase));
        if (canonicalStatus is null)
        {
            return BadRequest(new
            {
                message = "Status must be Pending, Approved, or Rejected."
            });
        }
        invoice.Status = canonicalStatus;
        await _context.SaveChangesAsync();
        return Ok(invoice);
    }
}
