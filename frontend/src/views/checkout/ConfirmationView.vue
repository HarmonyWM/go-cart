<script setup lang="ts">
import { useCheckoutStore } from '../../stores/checkoutStore'
import { useRouter } from 'vue-router'

const checkout = useCheckoutStore()
const router = useRouter()

const order = checkout.placedOrder
if (!order) router.push('/')

const isPersonal = order?.method === 'PersonalShopper'

function goToTracking() {
  router.push(`/orders/${order!.orderNumber}`)
}

function goHome() {
  checkout.reset()
  router.push('/')
}
</script>

<template>
  <div class="page" v-if="order">
    <div class="confirm-hero">
      <div class="confirm-icon">✅</div>
      <div class="confirm-title">Order Placed!</div>
      <div class="order-number">{{ order.orderNumber }}</div>
      <div class="confirm-sub">Save this number to track your order.</div>
    </div>

    <div class="card mb-12">
      <div class="summary-row">
        <span class="text-secondary">Method</span>
        <span class="fw-600">{{ isPersonal ? '🧑🛒 Personal Shopper' : '🛒 Shop Myself' }}</span>
      </div>
      <div class="summary-row">
        <span class="text-secondary">Total Paid</span>
        <span class="fw-600">R{{ order.grandTotal.toFixed(2) }}</span>
      </div>
      <div class="summary-row" v-if="checkout.paymentReference">
        <span class="text-secondary">Payment Ref</span>
        <span class="fw-600 ref">{{ checkout.paymentReference }}</span>
      </div>
      <div class="summary-row">
        <span class="text-secondary">Status</span>
        <span class="badge badge-green">{{ order.status }}</span>
      </div>
    </div>

    <div v-if="isPersonal && order.delivery" class="card mb-12">
      <div class="field-label">Delivering to</div>
      <div class="fw-600">{{ order.delivery.address }}</div>
      <div class="text-secondary">{{ order.delivery.deliveryWindow }}</div>
    </div>

    <div v-if="!isPersonal" class="card mb-12 self-shop-card">
      <div class="field-label">Your Shopping List</div>
      <div v-for="basket in order.baskets" :key="basket.storeId" class="self-store">
        <div class="self-store-name">📍 {{ basket.storeName }}</div>
        <div v-for="item in basket.items" :key="item.productId" class="self-item">
          <span>{{ item.productName }} × {{ item.quantity }}</span>
          <span>R{{ item.lineTotal.toFixed(2) }}</span>
        </div>
      </div>
    </div>

    <div class="demo-notice mb-12">
      ⚠️ DEMO — Payment and shopper assignment are simulated. No real transaction occurred.
    </div>

    <button v-if="isPersonal" class="btn btn-primary btn-full mb-10" @click="goToTracking">
      📦 Track My Order
    </button>
    <button class="btn btn-outline btn-full" @click="goHome">← Back to Home</button>
  </div>
</template>

<style scoped>
.confirm-hero { text-align: center; padding: 24px 0 20px; }
.confirm-icon { font-size: 56px; margin-bottom: 12px; }
.confirm-title { font-size: 26px; font-weight: 800; margin-bottom: 8px; }
.order-number { font-size: 20px; font-weight: 700; color: var(--mm-green-dark); background: var(--mm-green-light); padding: 8px 20px; border-radius: 20px; display: inline-block; margin-bottom: 8px; letter-spacing: 1px; }
.confirm-sub { font-size: 14px; color: var(--mm-text-secondary); }
.mb-12 { margin-bottom: 12px; }
.mb-10 { margin-bottom: 10px; }
.summary-row { display: flex; justify-content: space-between; align-items: center; padding: 8px 0; border-bottom: 1px solid var(--mm-border); }
.summary-row:last-child { border-bottom: none; }
.fw-600 { font-weight: 600; }
.ref { font-size: 12px; font-family: monospace; color: var(--mm-text-secondary); }
.field-label { font-size: 11px; font-weight: 700; color: var(--mm-text-secondary); text-transform: uppercase; margin-bottom: 8px; }
.self-shop-card { background: var(--mm-surface); }
.self-store { margin-bottom: 12px; }
.self-store-name { font-weight: 700; font-size: 14px; margin-bottom: 6px; }
.self-item { display: flex; justify-content: space-between; font-size: 13px; padding: 4px 0; border-bottom: 1px solid var(--mm-border); }
.demo-notice { background: #FFF3E0; color: var(--mm-orange); padding: 10px 14px; border-radius: var(--mm-radius-sm); font-size: 12px; }
</style>
