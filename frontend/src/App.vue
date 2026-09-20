<script setup>
import { onMounted, ref } from 'vue'

import {
  getInvoices,
  createInvoice,
  updateInvoiceStatus,
} from './services/invoiceService'


// -------------------------------------------------------
// APPLICATION STATE
// -------------------------------------------------------

// ref() creates reactive Vue state.
//
// When a ref changes, Vue updates the interface.

const invoices = ref([])

const loading = ref(false)
const saving = ref(false)

const error = ref('')
const success = ref('')

// Stores the ID of the invoice currently being updated.
//
// null means no status update is running.
const updatingId = ref(null)


// -------------------------------------------------------
// FORM STATE
// -------------------------------------------------------

const form = ref({
  invoiceNumber: '',
  supplierName: '',
  amount: '',
  currency: 'HUF',
})


// -------------------------------------------------------
// LOAD INVOICES
// -------------------------------------------------------

// Calls GET /api/invoices.
async function loadInvoices() {
  loading.value = true
  error.value = ''

  try {
    invoices.value = await getInvoices()
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}


// -------------------------------------------------------
// CREATE INVOICE
// -------------------------------------------------------

// Calls POST /api/invoices.
async function handleCreateInvoice() {
  saving.value = true

  error.value = ''
  success.value = ''

  try {
    await createInvoice({
      invoiceNumber: form.value.invoiceNumber.trim(),
      supplierName: form.value.supplierName.trim(),
      amount: Number(form.value.amount),
      currency: form.value.currency,
    })

    // Reset the form after a successful request.
    form.value = {
      invoiceNumber: '',
      supplierName: '',
      amount: '',
      currency: 'HUF',
    }

    // Refresh the list.
    await loadInvoices()

    if (!error.value) {
      success.value = 'Invoice created successfully.'
    }
  } catch (err) {
    error.value = err.message
  } finally {
    saving.value = false
  }
}


// -------------------------------------------------------
// UPDATE STATUS
// -------------------------------------------------------

// Calls PATCH /api/invoices/{id}/status.
async function handleUpdateStatus(id, status) {
  updatingId.value = id

  error.value = ''
  success.value = ''

  try {
    await updateInvoiceStatus(id, status)

    await loadInvoices()

    if (!error.value) {
      success.value = `Invoice updated to ${status}.`
    }
  } catch (err) {
    error.value = err.message
  } finally {
    updatingId.value = null
  }
}


// -------------------------------------------------------
// FORMAT AMOUNT
// -------------------------------------------------------

// Converts a number into a readable amount.
//
// Example:
// 12500 HUF -> 12,500 HUF
function formatAmount(amount, currency) {
  return `${Number(amount).toLocaleString('en-US')} ${currency}`
}


// -------------------------------------------------------
// COMPONENT INITIALIZATION
// -------------------------------------------------------

// onMounted runs when the component appears.
//
// We automatically load invoices when the page opens.
onMounted(loadInvoices)
</script>


<template>
  <main class="container">

    <!-- APPLICATION HEADER -->

    <header class="header">
      <div>
        <p class="eyebrow">
          FINANCEFLOW ENTERPRISE
        </p>

        <h1>Invoice Management</h1>

        <p class="subtitle">
          Create, review and manage supplier invoices.
        </p>
      </div>

      <button
        class="secondary-button"
        :disabled="loading"
        @click="loadInvoices"
      >
        {{ loading ? 'Loading...' : 'Refresh' }}
      </button>
    </header>


    <!-- ERROR AND SUCCESS MESSAGES -->

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


    <!-- CREATE INVOICE FORM -->

    <section class="card">
      <h2>Create invoice</h2>

      <form
        class="invoice-form"
        @submit.prevent="handleCreateInvoice"
      >

        <label>
          Invoice number

          <input
            v-model.trim="form.invoiceNumber"
            type="text"
            required
            placeholder="INV-003"
          />
        </label>


        <label>
          Supplier name

          <input
            v-model.trim="form.supplierName"
            type="text"
            required
            placeholder="Example Supplier"
          />
        </label>


        <label>
          Amount

          <input
            v-model="form.amount"
            type="number"
            min="0.01"
            step="0.01"
            required
            placeholder="12500"
          />
        </label>


        <label>
          Currency

          <select v-model="form.currency">
            <option value="HUF">HUF</option>
            <option value="EUR">EUR</option>
            <option value="USD">USD</option>
          </select>
        </label>


        <button
          class="primary-button"
          type="submit"
          :disabled="saving"
        >
          {{ saving ? 'Saving...' : 'Create invoice' }}
        </button>

      </form>
    </section>


    <!-- INVOICE LIST -->

    <section class="card">

      <div class="section-header">
        <h2>Invoices</h2>

        <span class="invoice-count">
          {{ invoices.length }} total
        </span>
      </div>


      <!-- Loading state -->

      <p v-if="loading" class="empty-message">
        Loading invoices...
      </p>


      <!-- Empty state -->

      <p
        v-else-if="invoices.length === 0"
        class="empty-message"
      >
        No invoices found. Create your first invoice above.
      </p>


      <!-- Invoice table -->

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

              <td>
                {{ invoice.id }}
              </td>


              <td class="invoice-number">
                {{ invoice.invoiceNumber }}
              </td>


              <td>
                {{ invoice.supplierName }}
              </td>


              <td>
                {{ formatAmount(invoice.amount, invoice.currency) }}
              </td>


              <td>
                <span
                  class="status"
                  :class="invoice.status.toLowerCase()"
                >
                  {{ invoice.status }}
                </span>
              </td>


              <td>
                <div class="actions">

                  <button
                    class="approve-button"
                    :disabled="
                      updatingId !== null ||
                      invoice.status === 'Approved'
                    "
                    @click="
                      handleUpdateStatus(invoice.id, 'Approved')
                    "
                  >
                    Approve
                  </button>


                  <button
                    class="reject-button"
                    :disabled="
                      updatingId !== null ||
                      invoice.status === 'Rejected'
                    "
                    @click="
                      handleUpdateStatus(invoice.id, 'Rejected')
                    "
                  >
                    Reject
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
.container {
  max-width: 1150px;
  margin: 0 auto;
  padding: 40px 24px;
  color: #172033;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
  margin-bottom: 32px;
}

.eyebrow {
  color: #2563eb;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 2px;
}

h1 {
  margin: 8px 0;
  font-size: 34px;
}

h2 {
  margin: 0 0 24px;
  font-size: 22px;
}

.subtitle {
  color: #64748b;
}

.card {
  background: white;
  padding: 28px;
  margin-bottom: 24px;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  box-shadow: 0 4px 20px rgba(15, 23, 42, 0.04);
}

.invoice-form {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 18px;
}

label {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-weight: 600;
}

input,
select {
  padding: 12px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font: inherit;
}

button {
  padding: 11px 16px;
  border: 0;
  border-radius: 8px;
  color: white;
  font-weight: 600;
  cursor: pointer;
}

button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.primary-button {
  background: #2563eb;
  align-self: end;
}

.secondary-button {
  background: #e2e8f0;
  color: #172033;
}

.approve-button {
  background: #15803d;
}

.reject-button {
  background: #b91c1c;
}

.actions {
  display: flex;
  gap: 8px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.section-header h2 {
  margin: 0;
}

.invoice-count {
  color: #64748b;
  font-size: 14px;
}

.table-wrapper {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

th,
td {
  padding: 14px 10px;
  border-bottom: 1px solid #e2e8f0;
}

th {
  color: #64748b;
  font-size: 13px;
}

.invoice-number {
  font-weight: 600;
}

.status {
  display: inline-block;
  padding: 6px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 700;
}

.pending {
  background: #fef3c7;
  color: #92400e;
}

.approved {
  background: #dcfce7;
  color: #166534;
}

.rejected {
  background: #fee2e2;
  color: #991b1b;
}

.message {
  padding: 14px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.error-message {
  background: #fee2e2;
  color: #991b1b;
}

.success-message {
  background: #dcfce7;
  color: #166534;
}

.empty-message {
  color: #64748b;
  padding: 20px 0;
}

@media (max-width: 700px) {
  .header {
    flex-direction: column;
    align-items: flex-start;
  }

  .invoice-form {
    grid-template-columns: 1fr;
  }
}
</style>