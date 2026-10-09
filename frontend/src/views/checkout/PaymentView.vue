<script setup lang="ts">
import { ref, computed } from 'vue'
import { useCheckoutStore } from '../../stores/checkoutStore'
import { useListStore } from '../../stores/listStore'
import { api, type PlaceOrderRequest } from '../../services/api'
import { useRouter } from 'vue-router'

const checkout = useCheckoutStore()
const listStore = useListStore()
const router = useRouter()

const opt = checkout.selectedOption
if (!opt) router.push('/optimise')

const loading = ref(false)
const error = ref('')
const selectedMethod = ref('card')

const total = computed(() => opt ? checkout.grandTotal(opt) : 0)
const isPersonal = computed(() => checkout.method === 'PersonalShopper')

async function pay() {
  if (!opt) return
  loading.value = true
  error.value = ''
  try {
    // Build order request
    const req: PlaceOrderRequest = {
      method: checkout.method,
      baskets: opt.storeBaskets.map(b => ({
        storeId: b.storeId, storeName: b.storeName,
        retailerName: b.retailerName, color: b.color,
        items: b.items.map(i => ({
          productId: i.productId, productName: i.productName,
          brand: i.brand, quantity: i.quantity,
          unitPrice: i.unitPrice, lineTotal: i.lineTotal,
          hasDeal: i.hasDeal, originalPrice: i.originalPrice,
          isSubstituted: false, substitutionNote: undefined
        })),
        subTotal: b.subTotal, distanceKm: b.distanceKm, travelCost: b.travelCost
      })),
      productTotal: opt.productTotal,
      travelCost: opt.travelCost,
      deliveryFee: isPersonal.value ? checkout.DELIVERY_FEE : 0,
      serviceFee: isPersonal.value ? checkout.SERVICE_FEE : 0,
      grandTotal: total.value,
      delivery: isPersonal.value ? checkout.delivery : undefined
    }

    const order = await api.placeOrder(req)

    // Process demo payment
    const payment = await api.demoPayment(order.id, selectedMethod.value)
    checkout.paymentReference = payment.reference
    checkout.placedOrder = order
    listStore.clearList()
    router.push('/checkout/confirmation')
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Failed to place order. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="page" v-if="opt">
    <div class="co-header">
      <button class="back-btn" @click="router.push('/checkout/review')">← Back</button>
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">Payment</h1>
    </div>

    <div class="step-indicator">Step 4 of 4</div>

    <div class="demo-banner">
      ⚠️ DEMO PAYMENT — No real money will be charged. This is a prototype.
    </div>

    <div class="card mb-14">
      <div class="field-label">Amount to pay</div>
      <div class="amount">R{{ total.toFixed(2) }}</div>
    </div>

    <div class="card mb-14">
      <div class="field-label">Payment Method</div>
      <div class="method-options">
        <button
          v-for="m in [{ v: 'card', l: '💳 Card' }, { v: 'eft', l: '🏦 EFT' }, { v: 'cash', l: '💵 Cash on Delivery' }]"
          :key="m.v"
          class="method-opt"
          :class="{ active: selectedMethod === m.v }"
          @click="selectedMethod = m.v"
        >{{ m.l }}</button>
      </div>
      <div class="demo-note">All payment methods are simulated in this prototype.</div>
    </div>

    <div v-if="selectedMethod === 'card'" class="card mb-14">
      <div class="field-label">Card Details (Demo)</div>
      <input value="4242 4242 4242 4242" readonly class="demo-input" placeholder="Card number" />
      <div class="card-row">
        <input value="12/28" readonly class="demo-input" placeholder="MM/YY" />
        <input value="123" readonly class="demo-input" placeholder="CVV" />
      </div>
      <div class="demo-note">Pre-filled demo card — not a real transaction.</div>
    </div>

    <div v-if="error" class="error-card">{{ error }}</div>

    <button class="btn btn-primary btn-full" :disabled="loading" @click="pay">
      <span v-if="loading"><span class="spinner-sm" /> Processing…</span>
      <span v-else>Place Order — R{{ total.toFixed(2) }}</span>
    </button>
  </div>
</template>

<style scoped>
.co-header { margin-bottom: 8px; }
.back-btn { background: none; border: none; color: var(--mm-green-dark); font-size: 15px; font-weight: 600; cursor: pointer; padding: 0; margin-bottom: 4px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; margin-bottom: 4px; }
.step-indicator { font-size: 12px; color: var(--mm-text-secondary); margin-bottom: 12px; }
.mb-14 { margin-bottom: 14px; }
.demo-banner { background: #FFF3E0; border: 1px solid var(--mm-orange); color: var(--mm-orange); padding: 10px 14px; border-radius: var(--mm-radius-sm); font-size: 13px; font-weight: 600; margin-bottom: 14px; }
.field-label { font-size: 11px; font-weight: 700; color: var(--mm-text-secondary); text-transform: uppercase; margin-bottom: 8px; }
.amount { font-size: 32px; font-weight: 800; color: var(--mm-green-dark); }
.method-options { display: flex; flex-direction: column; gap: 8px; margin-bottom: 8px; }
.method-opt { padding: 12px 16px; border-radius: var(--mm-radius-sm); border: 2px solid var(--mm-border); background: var(--mm-card); font-size: 15px; font-weight: 500; cursor: pointer; text-align: left; transition: all 0.15s; }
.method-opt.active { border-color: var(--mm-green); background: var(--mm-green-light); font-weight: 700; }
.demo-note { font-size: 11px; color: var(--mm-text-secondary); margin-top: 6px; }
.demo-input { margin-bottom: 8px; background: var(--mm-surface); color: var(--mm-text-secondary); }
.card-row { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.error-card { background: #FFEBEE; color: var(--mm-red); padding: 12px; border-radius: var(--mm-radius-sm); font-size: 14px; margin-bottom: 12px; }
.btn:disabled { opacity: 0.5; cursor: not-allowed; }
.spinner-sm { display: inline-block; width: 14px; height: 14px; border: 2px solid rgba(255,255,255,0.4); border-top-color: #fff; border-radius: 50%; animation: spin 0.7s linear infinite; vertical-align: middle; margin-right: 6px; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
