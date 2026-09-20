// This file contains the HTTP requests used by Vue frontend.
//
// The '/api' prefix is forwarded to ASP.NET Core by the proxy configured in vite.config.js.

const API_URL = '/api/invoices'


// Read an error response from the backend.
//
// ASP.NET Core may return:
// - a JSON object with a "message" property,
// - validation errors,
// - or an empty response.
//
// This helper gives the user a useful error message.
async function getErrorMessage(response) {
  const fallback = `Request failed: HTTP ${response.status}`

  try {
    const data = await response.json()

    if (data.message) {
      return data.message
    }

    if (data.errors) {
      return Object.values(data.errors)
        .flat()
        .join(' ')
    }

    if (data.title) {
      return data.title
    }
  } catch {
    // The response was not JSON.
  }

  return fallback
}


// GET /api/invoices
//
// Retrieve all invoices from ASP.NET Core.
export async function getInvoices() {
  const response = await fetch(API_URL)

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return await response.json()
}


// POST /api/invoices
//
// Create a new invoice.
export async function createInvoice(invoice) {
  const response = await fetch(API_URL, {
    method: 'POST',

    headers: {
      'Content-Type': 'application/json',
    },

    body: JSON.stringify(invoice),
  })

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return await response.json()
}


// PATCH /api/invoices/{id}/status
//
// Update an existing invoice's status.
export async function updateInvoiceStatus(id, status) {
  const response = await fetch(`${API_URL}/${id}/status`, {
    method: 'PATCH',

    headers: {
      'Content-Type': 'application/json',
    },

    body: JSON.stringify({
      status: status,
    }),
  })

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  return await response.json()
}