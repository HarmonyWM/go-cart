<script setup lang="ts">
import { useCheckoutStore } from '../../stores/checkoutStore'
import { useRouter } from 'vue-router'
import { computed } from 'vue'

const checkout = useCheckoutStore()
const router = useRouter()

if (!checkout.selectedOption) router.push('/optimise')

const windows = [
  '08:00 – 10:00', '10:00 – 12:00', '12:00 – 14:00',
  '14:00 – 16:00', '16:00 – 18:00', '18:00 – 20:00',
]

const valid = computed(() =>
  checkout.delivery.address.trim().length > 5 &&
  checkout.delivery.contactNumber.trim().length >= 10 &&
  checkout.delivery.deliveryWindow !== ''
)
</script>

<template>
  <div class="page">
    <div class="co-header">
      <button class="back-btn" @click="router.push('/checkout/method')">← Back</button>
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">Delivery Details</h1>
    </div>

    <div class="step-indicator">Step 2 of 4</div>

    <div class="card mb-14">
      <div class="field-label">Delivery Address *</div>
      <input v-model="checkout.delivery.address" placeholder="e.g. 12 Vilakazi St, Orlando West, Soweto" />
    </div>

    <div class="card mb-14">
      <div class="field-label">Contact Number *</div>
      <input v-model="checkout.delivery.contactNumber" type="tel" placeholder="e.g. 0821234567" />
    </div>

    <div class="card mb-14">
      <div class="field-label">Preferred Delivery Window *</div>
      <div class="window-grid">
        <button
          v-for="w in windows" :key="w"
          class="window-btn"
          :class="{ active: checkout.delivery.deliveryWindow === w }"
          @click="checkout.delivery.deliveryWindow = w"
        >{{ w }}</button>
      </div>
    </div>

    <div class="card mb-14">
      <div class="field-label">Delivery Notes (optional)</div>
      <input v-model="checkout.delivery.deliveryNotes" placeholder="e.g. Gate code, leave at door…" />
    </div>

    <div class="fee-summary card mb-14">
      <div class="fee-row">
        <span>Delivery fee</span>
        <span>R{{ checkout.DELIVERY_FEE.toFixed(2) }}</span>
      </div>
      <div class="fee-row">
        <span>Service fee</span>
        <span>R{{ checkout.SERVICE_FEE.toFixed(2) }}</span>
      </div>
      <div class="fee-note">Fees are shown before you confirm your order.</div>
    </div>

    <button class="btn btn-primary btn-full" :disabled="!valid" @click="router.push('/checkout/review')">
      Continue to Review →
    </button>
  </div>
</template>

<style scoped>
.co-header { margin-bottom: 8px; }
.back-btn { background: none; border: none; color: var(--mm-green-dark); font-size: 15px; font-weight: 600; cursor: pointer; padding: 0; margin-bottom: 4px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; margin-bottom: 4px; }
.step-indicator { font-size: 12px; color: var(--mm-text-secondary); margin-bottom: 20px; }
.mb-14 { margin-bottom: 14px; }
.field-label { font-size: 12px; font-weight: 700; color: var(--mm-text-secondary); text-transform: uppercase; letter-spacing: 0.3px; margin-bottom: 8px; }
.window-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.window-btn { padding: 10px; border-radius: var(--mm-radius-sm); border: 2px solid var(--mm-border); background: var(--mm-card); font-size: 13px; font-weight: 500; cursor: pointer; transition: all 0.15s; }
.window-btn.active { border-color: var(--mm-green); background: var(--mm-green-light); color: var(--mm-green-dark); font-weight: 700; }
.fee-summary { background: var(--mm-surface); }
.fee-row { display: flex; justify-content: space-between; font-size: 14px; padding: 4px 0; }
.fee-note { font-size: 12px; color: var(--mm-text-secondary); margin-top: 8px; }
.btn:disabled { opacity: 0.4; cursor: not-allowed; }
</style>
