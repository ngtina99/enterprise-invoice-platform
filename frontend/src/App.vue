<script setup>
import { computed, nextTick, onMounted, ref } from 'vue'

import {
  getInvoices,
  createInvoice,
  updateInvoiceStatus,
} from './services/invoiceService'

const invoices = ref([])

const loading = ref(false)
const saving = ref(false)

const error = ref('')
const success = ref('')
const updatingId = ref(null)
const updatingStatus = ref('')
const fieldErrors = ref({})
const invoiceForm = ref(null)
const loadError = ref('')
const hasLoaded = ref(false)

const busy = computed(() => loading.value || saving.value || updatingId.value !== null)
const emptyForm = () => ({ invoiceNumber: '', supplierName: '', amount: '', currency: 'HUF' })
const form = ref(emptyForm())

async function focusInvalidField() {
  await nextTick()
  invoiceForm.value?.querySelector('[aria-invalid="true"]')?.focus()
}

// Keep existing rows visible while requesting the latest saved invoices.
async function loadInvoices(afterSave = false) {
  if (loading.value) return
  loading.value = true
  loadError.value = ''
  try {
    invoices.value = await getInvoices()
    hasLoaded.value = true
  } catch (err) {
    loadError.value = afterSave
      ? `Your change was saved, but the list could not be refreshed. Do not submit it again. Use Refresh to retry. ${err.message}`
      : `Could not refresh invoices. Use Refresh to retry. ${err.message}`
  } finally {
    loading.value = false
  }
}

// Match creation rules locally and focus the first field that needs attention.
async function validateForm() {
  const errors = {}
  const number = form.value.invoiceNumber.trim()
  const supplier = form.value.supplierName.trim()
  const amount = Number(form.value.amount)
  if (!number || number.length > 50) errors.invoiceNumber = 'Enter an invoice number of 1–50 characters.'
  if (!supplier || supplier.length > 200) errors.supplierName = 'Enter a supplier name of 1–200 characters.'
  if (!Number.isFinite(amount) || amount < 0.01 || amount > 999999999999.99) {
    errors.amount = 'Enter an amount between 0.01 and 999,999,999,999.99.'
  } else if (Math.abs(amount * 100 - Math.round(amount * 100)) > 0.001) {
    errors.amount = 'Use no more than two decimal places.'
  }
  if (!/^[A-Z]{3}$/.test(form.value.currency)) errors.currency = 'Choose a currency.'
  fieldErrors.value = errors
  await focusInvalidField()
  return Object.keys(errors).length === 0
}

// Show server validation beside its field and keep unrecognized errors in the alert.
async function showCreateError(err) {
  const errors = {}
  for (const [key, messages] of Object.entries(err.fieldErrors || {})) {
    const field = Object.keys(form.value).find(name => name.toLowerCase() === key.toLowerCase())
    if (field) errors[field] = [].concat(messages).join(' ')
  }
  fieldErrors.value = errors
  error.value = err.message
  await focusInvalidField()
}

// Add the saved invoice immediately so a failed refresh cannot encourage duplicate submission.
async function handleCreateInvoice() {
  if (busy.value) return
  error.value = ''
  success.value = ''
  if (!await validateForm()) return
  saving.value = true
  try {
    const invoice = await createInvoice({
      invoiceNumber: form.value.invoiceNumber.trim(),
      supplierName: form.value.supplierName.trim(),
      amount: Number(form.value.amount),
      currency: form.value.currency,
    })
    invoices.value.push(invoice)
    form.value = emptyForm()
    fieldErrors.value = {}
    success.value = `Invoice ${invoice.invoiceNumber} created successfully.`
    await loadInvoices(true)
  } catch (err) {
    await showCreateError(err)
  } finally {
    saving.value = false
  }
}

// Identify the active action and apply the saved status before refreshing the list.
async function handleUpdateStatus(id, status) {
  if (busy.value) return
  updatingId.value = id
  updatingStatus.value = status
  error.value = ''
  success.value = ''
  try {
    const updated = await updateInvoiceStatus(id, status)
    invoices.value = invoices.value.map(invoice => invoice.id === id ? updated : invoice)
    success.value = `Invoice ${updated.invoiceNumber} updated to ${status}.`
    await loadInvoices(true)
  } catch (err) {
    error.value = err.message
  } finally {
    updatingId.value = null
    updatingStatus.value = ''
  }
}

// Format the amount with grouping separators and its currency code.
function formatAmount(amount, currency) {
  return `${Number(amount).toLocaleString('en-US')} ${currency}`
}

onMounted(loadInvoices)
</script>

<template>
  <main class="container">

    <header class="header">
      <div>
        <h1>Invoice Management</h1>

        <p class="subtitle">
          Create, review and manage supplier invoices.
        </p>
      </div>

      <button
        class="secondary-button"
        :disabled="busy"
        @click="loadInvoices()"
      >
        {{ loading ? 'Loading...' : 'Refresh' }}
      </button>
    </header>

    <div
      v-if="error"
      class="message error-message"
      role="alert"
    >
      {{ error }}
    </div>

    <div
      v-if="success"
      class="message success-message"
      role="status"
    >
      {{ success }}
    </div>

    <div v-if="loadError" class="message error-message" role="alert">{{ loadError }}</div>

    <section class="workspace-section">
      <h2>Create invoice</h2>

      <form
        class="invoice-form"
        ref="invoiceForm"
        novalidate
        @submit.prevent="handleCreateInvoice"
      >

        <label>
          Invoice number

          <input
            v-model.trim="form.invoiceNumber"
            :disabled="saving"
            :aria-invalid="Boolean(fieldErrors.invoiceNumber)"
            :aria-describedby="fieldErrors.invoiceNumber ? 'invoiceNumber-error' : undefined"
            @input="delete fieldErrors.invoiceNumber"
            type="text"
            required
            placeholder="e.g. INV-003"
          />
        <span v-if="fieldErrors.invoiceNumber" id="invoiceNumber-error" class="field-error">{{ fieldErrors.invoiceNumber }}</span>
        </label>

        <label>
          Supplier name

          <input
            v-model.trim="form.supplierName"
            :disabled="saving"
            :aria-invalid="Boolean(fieldErrors.supplierName)"
            :aria-describedby="fieldErrors.supplierName ? 'supplierName-error' : undefined"
            @input="delete fieldErrors.supplierName"
            type="text"
            required
            placeholder="e.g. Example Supplier Company"
          />
        <span v-if="fieldErrors.supplierName" id="supplierName-error" class="field-error">{{ fieldErrors.supplierName }}</span>
        </label>

        <label>
          Amount

          <input
            v-model="form.amount"
            :disabled="saving"
            :aria-invalid="Boolean(fieldErrors.amount)"
            :aria-describedby="fieldErrors.amount ? 'amount-error' : undefined"
            @input="delete fieldErrors.amount"
            type="number"
            min="0.01"
            step="0.01"
            required
            placeholder="e.g. 12500"
          />
        <span v-if="fieldErrors.amount" id="amount-error" class="field-error">{{ fieldErrors.amount }}</span>
        </label>

        <label>
          Currency

          <select v-model="form.currency"
            :disabled="saving"
            :aria-invalid="Boolean(fieldErrors.currency)"
            :aria-describedby="fieldErrors.currency ? 'currency-error' : undefined"
            @input="delete fieldErrors.currency">
            <option value="HUF">HUF</option>
            <option value="EUR">EUR</option>
            <option value="USD">USD</option>
          </select>
        <span v-if="fieldErrors.currency" id="currency-error" class="field-error">{{ fieldErrors.currency }}</span>
        </label>

        <button
          class="primary-button"
          type="submit"
          :disabled="busy"
        >
          {{ saving ? 'Saving...' : 'Create invoice' }}
        </button>

      </form>
    </section>

    <section class="workspace-section">

      <div class="section-header">
        <h2>Invoices</h2>

        <span class="invoice-count" role="status">
          {{ loading ? 'Refreshing…' : `${invoices.length} total` }}
        </span>
      </div>

      <p v-if="loading && invoices.length === 0" class="empty-message" role="status">
        Loading invoices...
      </p>

      <p
        v-else-if="invoices.length === 0"
        class="empty-message"
      >
        {{ !hasLoaded && loadError ? 'Invoices are unavailable. Use Refresh to try again.' : 'No invoices found. Create your first invoice above.' }}
      </p>

      <div v-else class="table-wrapper">

        <table>

          <thead>
            <tr>
              <th>ID</th>
              <th>Invoice number</th>
              <th>Supplier</th>
              <th>Amount</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>

          <tbody>

            <tr
              v-for="invoice in invoices"
              :key="invoice.id"
            >

              <td data-label="ID">
                {{ invoice.id }}
              </td>

              <td class="invoice-number" data-label="Invoice">
                {{ invoice.invoiceNumber }}
              </td>

              <td data-label="Supplier">
                {{ invoice.supplierName }}
              </td>

              <td data-label="Amount">
                {{ formatAmount(invoice.amount, invoice.currency) }}
              </td>

              <td data-label="Status">
                <span
                  class="status"
                  :class="invoice.status.toLowerCase()"
                >
                  {{ invoice.status }}
                </span>
              </td>

              <td data-label="Actions">
                <div class="actions">

                  <button
                    class="approve-button"
                    :disabled="
                      busy ||
                      invoice.status === 'Approved'
                    "
                    @click="
                      handleUpdateStatus(invoice.id, 'Approved')
                    "
                  >
                    {{ updatingId === invoice.id && updatingStatus === 'Approved' ? 'Approving…' : 'Approve' }}
                  </button>

                  <button
                    class="reject-button"
                    :disabled="
                      busy ||
                      invoice.status === 'Rejected'
                    "
                    @click="
                      handleUpdateStatus(invoice.id, 'Rejected')
                    "
                  >
                    {{ updatingId === invoice.id && updatingStatus === 'Rejected' ? 'Rejecting…' : 'Reject' }}
                  </button>

                </div>
              </td>

            </tr>

          </tbody>

        </table>

      </div>

    </section>

  </main>
</template>

<style scoped>
.field-error {
  color: #e0a5a9;
  font-size: 16px;
  line-height: 1.5;
}

[aria-invalid="true"] {
  border-color: #e0a5a9;
}

.invoice-form label {
  align-self: start;
}

.invoice-form .primary-button {
  align-self: start;
  margin-top: 29px;
}

@media (max-width: 600px) {
  .table-wrapper {
    overflow: visible;
  }

  table, tbody, tr, td {
    display: block;
    width: 100%;
    white-space: normal;
  }

  thead {
    position: absolute;
    width: 1px;
    height: 1px;
    overflow: hidden;
    clip-path: inset(50%);
  }

  tbody tr {
    padding: 16px 0;
    border-bottom: 1px solid #293447;
  }

  tbody tr td {
    display: flex;
    align-items: baseline;
    justify-content: space-between;
    gap: 20px;
    padding: 8px 0;
    border: 0;
    overflow-wrap: anywhere;
    text-align: right;
  }

  td::before {
    content: attr(data-label);
    color: #98aac4;
    font-weight: 400;
    flex-shrink: 0;
    text-align: left;
  }

  .invoice-form .primary-button {
    margin-top: 0;
  }
}

.container {
  max-width: 1160px;
  margin: 0 auto;
  padding: 48px 32px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 24px;
  padding-bottom: 28px;
  border-bottom: 1px solid #293447;
}

h1 {
  margin: 0;
  font-size: clamp(28px, calc(4vw + 4px), 32px);
  font-weight: 550;
  letter-spacing: -0.7px;
  line-height: 1.2;
}

h2 {
  margin: 0 0 24px;
  font-size: 20px;
  font-weight: 550;
  letter-spacing: -0.2px;
}

.subtitle {
  margin: 8px 0 0;
  color: #98aac4;
  font-size: 18px;
  line-height: 1.6;
}

.workspace-section {
  padding: 28px 0 32px;
  border-bottom: 1px solid #293447;
}

.workspace-section:last-child {
  border-bottom: 0;
}

.invoice-form {
  display: grid;
  grid-template-columns: minmax(120px, 1fr) minmax(160px, 1.6fr) minmax(100px, 1fr) 96px auto;
  align-items: end;
  gap: 14px;
}

label {
  display: flex;
  flex-direction: column;
  gap: 9px;
  color: #b4c4da;
  font-size: 17px;
  font-weight: 500;
}

input,
select {
  width: 100%;
  min-width: 0;
  min-height: 40px;
  padding: 10px 12px;
  border: 1px solid #344257;
  border-radius: 4px;
  background: #141e2d;
  color: #e6edf8;
  font-size: 18px;
  transition: border-color 150ms ease;
}

select {
  appearance: none;
  padding-right: 28px;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='10' height='6' viewBox='0 0 10 6'%3E%3Cpath d='m1 1 4 4 4-4' fill='none' stroke='%2398aac4' stroke-width='1.5' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 8px center;
}

input::placeholder {
  color: #8297b4;
  font-size: 18px;
}

input:hover,
select:hover {
  border-color: #5275a3;
}

button {
  min-height: 40px;
  padding: 10px 16px;
  border: 1px solid #344257;
  border-radius: 4px;
  background: transparent;
  color: #d7e5f9;
  font-size: 17px;
  font-weight: 550;
  cursor: pointer;
  transition: background 150ms ease, border-color 150ms ease;
}

button:hover:not(:disabled) {
  background: #202e41;
  border-color: #638ec7;
}

button:focus-visible,
input:focus-visible,
select:focus-visible {
  outline: 2px solid #7db5ff;
  outline-offset: 3px;
}

button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.primary-button {
  white-space: nowrap;
  background: #315d96;
  border-color: #426da5;
  color: #ffffff;
}

.primary-button:hover:not(:disabled) {
  background: #3c6da9;
  border-color: #7294bf;
}

.secondary-button {
  flex-shrink: 0;
}

.actions {
  display: flex;
  gap: 8px;
}

.actions button {
  min-height: 34px;
  padding: 7px 8px;
  border-color: currentColor;
  font-size: 16px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
}

.section-header h2 {
  margin: 0;
}

.invoice-count {
  color: #a0b4cf;
  font-size: 16px;
  font-variant-numeric: tabular-nums;
}

.table-wrapper {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  white-space: nowrap;
  font-size: 17px;
}

th,
td {
  padding: 16px 12px;
  border-bottom: 1px solid #293447;
}

th {
  padding-top: 0;
  color: #98aac4;
  font-size: 16px;
  font-weight: 500;
}

th:first-child,
td:first-child {
  padding-left: 0;
  color: #98aac4;
}

th:last-child,
td:last-child {
  padding-right: 0;
}

td:nth-child(4) {
  font-variant-numeric: tabular-nums;
}

tbody tr:last-child td {
  border-bottom: 0;
}

tbody tr:hover {
  background: #172334;
}

.invoice-number {
  font-weight: 550;
  color: #e6edf8;
}

.status {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 5px 0;
  font-size: 15px;
  font-weight: 500;
}

.status::before {
  content: '';
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: currentColor;
}

.pending {
  color: #dbc28b;
}

.approve-button,
.approved,
.success-message {
  color: #a8c8ba;
}

.reject-button,
.rejected,
.error-message {
  color: #e0a5a9;
}

.message {
  padding: 14px 18px;
  border-left: 2px solid currentColor;
  background: #151f2d;
  margin-top: 24px;
  font-size: 17px;
  line-height: 1.6;
}

.empty-message {
  margin: 0;
  color: #98aac4;
  padding: 36px 0;
  text-align: center;
  font-size: 18px;
  line-height: 1.6;
}

@media (max-width: 1000px) {
  .invoice-form {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .primary-button {
    justify-self: start;
  }
}

@media (max-width: 600px) {
  .container {
    padding: 32px 16px;
  }

  .header {
    align-items: flex-start;
    gap: 16px;
    padding-bottom: 24px;
  }

  .workspace-section {
    padding: 24px 0;
  }

  .invoice-form {
    grid-template-columns: 1fr;
  }

  .primary-button {
    width: 100%;
  }
}

@media (prefers-reduced-motion: reduce) {
  button,
  input,
  select {
    transition: none;
  }
}
</style>
