const API_URL = '/api/invoices'

// Preserve field validation details while providing a readable fallback for other failures.
async function getApiError(response) {
  let data
  try {
    data = await response.json()
  } catch {
    // Empty or non-JSON bodies fall back to the HTTP status.
  }
  const message = data?.message || (data?.errors && Object.values(data.errors).flat().join(' '))
    || data?.title || `Request failed: HTTP ${response.status}`
  const error = new Error(message)
  error.fieldErrors = data?.errors || {}
  return error
}

async function requestInvoice(path = '', method = 'GET', body) {
  const options = { method }
  if (body !== undefined) {
    options.headers = { 'Content-Type': 'application/json' }
    options.body = JSON.stringify(body)
  }
  const response = await fetch(`${API_URL}${path}`, options)
  if (!response.ok) throw await getApiError(response)
  return response.json()
}

// Fetch all invoices and surface API errors to the caller.
export function getInvoices() {
  return requestInvoice()
}

// Send invoice form data and return the saved invoice.
export function createInvoice(invoice) {
  return requestInvoice('', 'POST', invoice)
}

// Patch only the invoice status and return the updated invoice.
export function updateInvoiceStatus(id, status) {
  return requestInvoice(`/${id}/status`, 'PATCH', { status })
}
