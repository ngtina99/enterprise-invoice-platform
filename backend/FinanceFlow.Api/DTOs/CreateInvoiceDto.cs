// Beimportáljuk a DataAnnotations névteret.
// Ez tartalmazza az adatellenőrzéshez használt attribútumokat,
using System.ComponentModel.DataAnnotations;

// Megadjuk, hogy az osztály a FinanceFlow.Api.DTOs névtérhez tartozik.
// A DTO (Data Transfer Object) az API-n keresztül érkező vagy küldött
// adatok továbbítására szolgál.
namespace FinanceFlow.Api.DTOs;

// Ez az osztály meghatározza, milyen adatokat várunk
// egy új számla létrehozásakor.
public class CreateInvoiceDto
{
    // Kötelező mező: az érték nem lehet null.
    [Required]
    [StringLength(50, MinimumLength = 1)]
    // A számla azonosítója/számlaszáma.
    // A string.Empty kezdőérték egy üres szöveg ("").
    public string InvoiceNumber { get; set; } = string.Empty;


    [Required]
    // A beszállító neve 1–200 karakter hosszú lehet.
    [StringLength(200, MinimumLength = 1)]
    public string SupplierName { get; set; } = string.Empty;


    // Az összegnek 0,01 és 999 999 999 999 között kell lennie.
    // A typeof(decimal) megadja, hogy decimal típusú értéket ellenőrzünk.
    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; set; }


    [Required]
    // Reguláris kifejezés (regex):
    // ^       = a szöveg eleje
    // [A-Z]   = egy nagybetű A és Z között
    // {3}     = pontosan három ilyen karakter
    // $       = a szöveg vége

    [RegularExpression("^[A-Z]{3}$")]
    // A számla pénzneme.
    // Alapértelmezett értéke HUF, ha nem adunk meg másikat.
    public string Currency { get; set; } = "HUF";
}