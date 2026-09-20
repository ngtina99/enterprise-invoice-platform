// This DTO defines the data the client sends when creating an invoice.
using FinanceFlow.Api.DTOs;
// Import the namespace containing the Invoice model.
using FinanceFlow.Api.Models;

// Import ASP.NET Core MVC (Model–View–Controller) types and attributes:
// ControllerBase, ApiController, Route, HttpGet, HttpPost, etc.
// This imports the Microsoft.AspNetCore.Mvc namespace, which contains ASP.NET Core controller functionality, including:

// ControllerBase
// [ApiController]
// [HttpGet]
// [HttpPost]
using Microsoft.AspNetCore.Mvc;

namespace FinanceFlow.Api.Controllers;

// Marks this class as an API controller.
// Among other features, ASP.NET Core automatically returns HTTP 400 Bad Request when model validation fails.
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
// : ControllerBase: inherits API controller functionality, such as Ok(), NotFound(), and CreatedAtAction().
public class InvoicesController : ControllerBase
{
    // A list that stores Invoice objects in memory.
    //
    // private: accessible only inside this class.
    // static: shared by ALL instances of InvoicesController.
    // readonly: the Invoices field cannot be reassigned after initialization.
    //
    // IMPORTANT: readonly does NOT make the list immutable.
    // We can still add and remove invoices.
    //
    // new(): creates a new List<Invoice>.
    private static readonly List<Invoice> Invoices = new();


    // An object used as a synchronization lock.
    //
    // Multiple HTTP requests can execute simultaneously.
    // This object helps prevent them from modifying or reading the shared invoice list at conflicting times.
    //
    // static: every controller instance uses the SAME lock.
    // readonly: the lock object cannot be reassigned.
    private static readonly object Sync = new();


    // Stores the ID that will be assigned to the next invoice.
    //
    // static: shared by all controller instances.
    // private: accessible only inside this class.
    //
    // The underscore is a common naming convention
    // for private fields.
    private static int _nextId = 1;


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
    // ActionResult<List<Invoice>>:
    // The action can return a list of invoices OR
    // an HTTP result such as Ok(...).
    //
    // GetAll(): method name; no parameters.
    public ActionResult<List<Invoice>> GetAll()
    {
        // Allow only one thread at a time to execute code
        // protected by this particular Sync object.
        //
        // Other requests using the same lock must wait.
        lock (Sync)
        {
            // Invoices.ToList():
            // Creates a NEW list containing the invoice references.
            //
            // This copies the list structure, but does not create
            // independent copies of the Invoice objects.
            //
            // Ok(...):
            // Returns HTTP 200 OK with the list as the response body.
            //
            // ASP.NET Core typically serializes the list to JSON.
            return Ok(Invoices.ToList());
        }
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

    // Returns either an Invoice or an HTTP result, such as 200 OK or 404 Not Found.
    //
    // int id:
    // ASP.NET Core obtains this value from the URL.
    public ActionResult<Invoice> GetById(int id)
    {
        // Protect access to the shared invoice list.
        lock (Sync)
        {
            // Search the list for the FIRST invoice
            // whose Id matches the requested ID.
            //
            // FirstOrDefault(...) is a LINQ method.
            //
            // i => i.Id == id
            // This is a lambda expression:
            // "For each invoice i, check whether its Id equals id."
            //
            // If no invoice matches, FirstOrDefault returns null
            // because Invoice is a class (reference type).
            var invoice = Invoices
                .FirstOrDefault(i => i.Id == id);


            // Check whether the search found an invoice.
            if (invoice == null)
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
            // ASP.NET Core typically converts it to JSON.
            return Ok(invoice);
        }
    }


    // -------------------------------------------------------
    // CREATE A NEW INVOICE
    // -------------------------------------------------------

    // Example HTTP request:
    // POST /api/invoices

    // Maps HTTP POST requests to this method.
    [HttpPost]

    // ActionResult<Invoice>:
    // The method returns an Invoice together with
    // an appropriate HTTP response.
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
    public ActionResult<Invoice> Create(
        CreateInvoiceDto request)
    {
        // Protect the shared list and ID counter.
        //
        // Without synchronization, two simultaneous requests
        // might interfere with invoice creation.
        lock (Sync)
        {
            // Create a NEW Invoice object.
            //
            // "new Invoice" constructs the object.
            // The { ... } block is an object initializer:
            // it assigns values to its properties.
            var invoice = new Invoice
            {
                // Assign the next available ID.
                //
                // _nextId++ means:
                // 1. Use the current value of _nextId.
                // 2. Increase _nextId by 1 afterward.
                //
                // Example:
                // First invoice: Id = 1, next ID becomes 2.
                // Second invoice: Id = 2, next ID becomes 3.
                Id = _nextId++,

                // Copy the invoice number from the DTO.
                InvoiceNumber = request.InvoiceNumber,

                // Copy the supplier name from the DTO.
                SupplierName = request.SupplierName,

                // Copy the amount from the DTO.
                Amount = request.Amount,

                // Copy the currency from the DTO.
                Currency = request.Currency
            };


            // Add the newly created invoice to the shared list.
            //
            // Because this is an in-memory list,
            // the data will be lost when the application restarts.
            Invoices.Add(invoice);


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
    }
}