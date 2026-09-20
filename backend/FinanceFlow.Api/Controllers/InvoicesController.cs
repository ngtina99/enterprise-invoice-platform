// This DTO defines the data the client sends when creating an invoice.
using FinanceFlow.Api.DTOs;

// Import the namespace containing the Invoice model.
using FinanceFlow.Api.Models;

// Import the namespace containing our Entity Framework Core database context.
using FinanceFlow.Api.Data;

// Import Entity Framework Core functionality, including ToListAsync().
using Microsoft.EntityFrameworkCore;

// Import ASP.NET Core MVC (Model–View–Controller) types and attributes:
//
// ControllerBase
// [ApiController]
// [Route]
// [HttpGet]
// [HttpPost]
//
// This namespace also provides HTTP response helpers such as:
// Ok(), NotFound(), and CreatedAtAction().
using Microsoft.AspNetCore.Mvc;

namespace FinanceFlow.Api.Controllers;

// Marks this class as an API controller.
//
// Among other features, ASP.NET Core automatically returns
// HTTP 400 Bad Request when model validation fails.
[ApiController]

// Defines the base URL for this controller.
//
// [controller] is replaced with the controller class name
// without the "Controller" suffix.
//
// InvoicesController -> Invoices
//
// Result: api/invoices
[Route("api/[controller]")]


// public: ASP.NET Core can access this class.
// class: declares a C# class.
// InvoicesController: the class name.
//
// : ControllerBase:
// Inherits API controller functionality, such as
// Ok(), NotFound(), and CreatedAtAction().
public class InvoicesController : ControllerBase
{
    // -------------------------------------------------------
    // DATABASE CONTEXT
    // -------------------------------------------------------

    // This field stores the database context used by this controller.
    //
    // private: accessible only inside this class.
    // readonly: the field cannot be reassigned after construction.
    //
    // FinanceFlowDbContext:
    // Our Entity Framework Core database context.
    //
    // Unlike the previous static List<Invoice>, this context
    // allows us to read and save invoices in the SQLite database.
    private readonly FinanceFlowDbContext _context;


    // This is the controller's constructor.
    //
    // ASP.NET Core uses dependency injection to provide
    // a FinanceFlowDbContext instance automatically.
    //
    // The DbContext must be registered in Program.cs
    // using AddDbContext<FinanceFlowDbContext>(...).
    //
    // context:
    // The database context provided by ASP.NET Core.
    //
    // _context = context:
    // Stores it in our private field so the controller's
    // methods can use it.
    public InvoicesController(FinanceFlowDbContext context)
    {
        _context = context;
    }


    // -------------------------------------------------------
    // GET ALL INVOICES
    // -------------------------------------------------------

    // Example HTTP request:
    // GET /api/invoices

    // Maps HTTP GET requests for the controller's base route
    // to the GetAll() method.
    [HttpGet]

    // public: ASP.NET Core can call this method.
    //
    // async:
    // Allows this method to use await for asynchronous operations.
    //
    // Task<ActionResult<List<Invoice>>>:
    // Represents an asynchronous operation that returns
    // either a list of invoices or an HTTP result.
    //
    // GetAll(): method name; no parameters.
    public async Task<ActionResult<List<Invoice>>> GetAll()
    {
        // _context:
        // Our FinanceFlowDbContext instance.
        //
        // _context.Invoices:
        // The DbSet<Invoice> defined in our DbContext.
        //
        // ToListAsync():
        // Executes the database query asynchronously
        // and returns the invoices as a List<Invoice>.
        //
        // await:
        // Asynchronously waits for the database query to finish.
        var invoices = await _context.Invoices.ToListAsync();


        // Ok(...):
        // Returns HTTP 200 OK with the invoices in the response body.
        //
        // ASP.NET Core normally serializes the list into JSON.
        return Ok(invoices);
    }


    // -------------------------------------------------------
    // GET ONE INVOICE BY ID
    // -------------------------------------------------------

    // Example HTTP request:
    // GET /api/invoices/1

    // "{id:int}" adds an ID parameter to the URL.
    //
    // id: name of the route parameter.
    // int: route constraint requiring an integer-shaped value.
    //
    // GET /api/invoices/1 -> id = 1
    //
    // A URL such as /api/invoices/abc does not match this route.
    [HttpGet("{id:int}")]

    // Task<ActionResult<Invoice>>:
    // Represents an asynchronous operation that returns
    // either an Invoice or an HTTP result.
    //
    // int id:
    // ASP.NET Core obtains this value from the URL.
    public async Task<ActionResult<Invoice>> GetById(int id)
    {
        // FindAsync(id):
        // Looks for an invoice using its primary key (Id).
        //
        // EF Core first checks whether the entity is already
        // tracked by this DbContext. Otherwise, it queries
        // the database.
        //
        // If the invoice does not exist, it returns null.
        var invoice = await _context.Invoices.FindAsync(id);


        // Check whether an invoice was found.
        if (invoice is null)
        {
            // Return HTTP 404 Not Found.
            //
            // Example:
            // GET /api/invoices/999
            //
            // If invoice 999 does not exist -> 404.
            return NotFound();
        }


        // An invoice was found.
        //
        // Return HTTP 200 OK with the invoice data.
        //
        // ASP.NET Core typically converts it to JSON.
        return Ok(invoice);
    }


    // -------------------------------------------------------
    // CREATE A NEW INVOICE
    // -------------------------------------------------------

    // Example HTTP request:
    // POST /api/invoices

    // Maps HTTP POST requests to this method.
    [HttpPost]

    // Task<ActionResult<Invoice>>:
    // The method performs asynchronous work and returns
    // an Invoice together with an appropriate HTTP response.
    //
    // CreateInvoiceDto request:
    // ASP.NET Core reads the incoming JSON request body
    // and converts it into a CreateInvoiceDto object.
    //
    // Because this controller has [ApiController],
    // a complex DTO parameter is inferred to come
    // from the request body.
    //
    // Data Annotation validation also runs automatically.
    public async Task<ActionResult<Invoice>> Create(
        CreateInvoiceDto request)
    {
        // Create a NEW Invoice object.
        //
        // "new Invoice" constructs the object.
        //
        // The { ... } block is an object initializer:
        // it assigns values to the object's properties.
        var invoice = new Invoice
        {
            // We do NOT assign Id manually.
            //
            // The database generates the primary key
            // when the invoice is inserted.

            // Copy the invoice number from the DTO.
            InvoiceNumber = request.InvoiceNumber,

            // Copy the supplier name from the DTO.
            SupplierName = request.SupplierName,

            // Copy the amount from the DTO.
            Amount = request.Amount,

            // Copy the currency from the DTO.
            Currency = request.Currency,

            // Every newly created invoice starts as Pending.
            //
            // The server controls this value rather than
            // accepting it from the creation DTO.
            Status = "Pending",

            // Record the creation time in UTC.
            //
            // DateTime.UtcNow:
            // Gets the current date and time in UTC.
            CreatedAt = DateTime.UtcNow
        };


        // Add the new invoice to the DbContext.
        //
        // EF Core starts tracking this invoice as a new entity.
        //
        // IMPORTANT:
        // Add() alone does not save the invoice to the database.
        _context.Invoices.Add(invoice);


        // Save the tracked changes to the database.
        //
        // SaveChangesAsync():
        // Executes the INSERT operation asynchronously.
        //
        // After a successful save, EF Core updates invoice.Id
        // with the primary key generated by the database.
        //
        // This is what makes the invoice persist in SQLite
        // instead of disappearing when the API restarts.
        await _context.SaveChangesAsync();


        // Return HTTP 201 Created.
        //
        // CreatedAtAction(...) also generates a Location
        // response header pointing to the new resource.
        //
        // nameof(GetById):
        // Gets the method name "GetById" safely.
        // If the method is renamed, nameof updates with it.
        //
        // new { id = invoice.Id }:
        // Creates an anonymous object containing the route
        // value required by GetById.
        //
        // If invoice.Id = 1, the generated Location
        // will typically be /api/invoices/1.
        //
        // invoice:
        // The newly created invoice is included
        // in the response body.
        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            invoice
        );
    }
    // -------------------------------------------------------
    // UPDATE INVOICE STATUS
    // -------------------------------------------------------

    // Example HTTP request:
    // PATCH /api/invoices/1/status
    //
    // PATCH:
    // Updates part of an existing resource instead of
    // replacing the entire invoice.
    //
    // "{id:int}/status":
    // The invoice ID comes from the URL.
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<Invoice>> UpdateStatus(
        int id,
        UpdateInvoiceStatusDto request)
    {
        // Find the invoice in the database by its primary key.
        var invoice = await _context.Invoices.FindAsync(id);

        // Return HTTP 404 if the invoice does not exist.
        if (invoice is null)
        {
            return NotFound(new
            {
                message = $"Invoice with ID {id} was not found."
            });
        }

        // Remove leading/trailing spaces.
        //
        // For example:
        // " Approved " -> "Approved"
        var status = request.Status?.Trim();

        // Allow only the statuses supported by our MVP.
        //
        // StringComparison.OrdinalIgnoreCase means
        // "approved" and "Approved" are treated equally.
        if (string.Equals(
            status,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            invoice.Status = "Pending";
        }
        else if (string.Equals(
            status,
            "Approved",
            StringComparison.OrdinalIgnoreCase))
        {
            invoice.Status = "Approved";
        }
        else if (string.Equals(
            status,
            "Rejected",
            StringComparison.OrdinalIgnoreCase))
        {
            invoice.Status = "Rejected";
        }
        else
        {
            // Return HTTP 400 for unsupported status values.
            return BadRequest(new
            {
                message = "Status must be Pending, Approved, or Rejected."
            });
        }

        // EF Core tracks the invoice returned by FindAsync.
        //
        // Changing invoice.Status marks the property as modified.
        // SaveChangesAsync persists the update to SQLite.
        await _context.SaveChangesAsync();

        // Return HTTP 200 OK with the updated invoice.
        return Ok(invoice);
    }
}