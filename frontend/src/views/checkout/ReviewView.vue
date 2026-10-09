<script setup lang="ts">
import { useCheckoutStore } from '../../stores/checkoutStore'
import { useRouter } from 'vue-router'
import { computed } from 'vue'

const checkout = useCheckoutStore()
const router = useRouter()

const opt = checkout.selectedOption
if (!opt) router.push('/optimise')

const isPersonal = computed(() => checkout.method === 'PersonalShopper')
const total = computed(() => opt ? checkout.grandTotal(opt) : 0)

const backRoute = computed(() =>
  isPersonal.value ? '/checkout/delivery' : '/checkout/method'
)
</script>

<template>
  <div class="page" v-if="opt">
    <div class="co-header">
      <button class="back-btn" @click="router.push(backRoute)">← Back</button>
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">Review Your Basket</h1>
    </div>

    <div class="step-indicator">Step 3 of 4</div>

    <!-- Method badge -->
    <div class="method-badge" :class="isPersonal ? 'personal' : 'self'">
      {{ isPersonal ? '🧑🛒 MaliMove Shops For Me' : '🛒 Shop Myself' }}
    </div>

    <!-- Baskets -->
    <div v-for="basket in opt.storeBaskets" :key="basket.storeId" class="basket-card card mb-12">
      <div class="basket-header" :style="{ borderLeftColor: basket.color }">
        <span class="basket-store">{{ basket.storeName }}</span>
        <span class="basket-retailer">{{ basket.retailerName }}</span>
      </div>
      <div v-for="item in basket.items" :key="item.productId" class="basket-item">
        <div class="item-left">
          <span class="item-name">{{ item.productName }}</span>
          <span class="item-brand text-secondary">{{ item.brand }} · ×{{ item.quantity }}</span>
        </div>
        <div class="item-right">
          <span v-if="item.hasDeal" class="item-original">R{{ item.originalPrice?.toFixed(2) }}</span>
          <span class="item-price">R{{ item.lineTotal.toFixed(2) }}</span>
          <span v-if="item.hasDeal" class="deal-chip">DEAL</span>
        </div>
      </div>
      <div class="basket-footer">
        <span class="text-secondary">Subtotal</span>
        <span class="fw-700">R{{ basket.subTotal.toFixed(2) }}</span>
      </div>
    </div>

    <!-- Cost breakdown -->
    <div class="card mb-12">
      <div class="cost-line">
        <span>Products</span><span>R{{ opt.productTotal.toFixed(2) }}</span>
      </div>
      <div class="cost-line">
        <span>Est. Travel</span><span>R{{ opt.travelCost.toFixed(2) }}</span>
      </div>
      <template v-if="isPersonal">
        <div class="cost-line">
          <span>Delivery fee</span><span>R{{ checkout.DELIVERY_FEE.toFixed(2) }}</span>
        </div>
        <div class="cost-line">
          <span>Service fee</span><span>R{{ checkout.SERVICE_FEE.toFixed(2) }}</span>
        </div>
      </template>
      <div class="divider" />
      <div class="cost-line total-line">
        <span>Total</span><span>R{{ total.toFixed(2) }}</span>
      </div>
    </div>

    <!-- Delivery summary -->
    <div v-if="isPersonal && checkout.delivery.address" class="card mb-12 delivery-summary">
      <div class="field-label">Delivering to</div>
      <div class="fw-600">{{ checkout.delivery.address }}</div>
      <div class="text-secondary">{{ checkout.delivery.contactNumber }}</div>
      <div class="text-secondary">Window: {{ checkout.delivery.deliveryWindow }}</div>
    </div>

    <button class="btn btn-primary btn-full" @click="router.push('/checkout/payment')">
      Confirm & Pay →
    </button>
    <div class="approval-note">By continuing you approve this basket and total.</div>
  </div>
</template>

<style scoped>
.co-header { margin-bottom: 8px; }
.back-btn { background: none; border: none; color: var(--mm-green-dark); font-size: 15px; font-weight: 600; cursor: pointer; padding: 0; margin-bottom: 4px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; margin-bottom: 4px; }
.step-indicator { font-size: 12px; color: var(--mm-text-secondary); margin-bottom: 12px; }
.mb-12 { margin-bottom: 12px; }
.method-badge { display: inline-block; padding: 6px 14px; border-radius: 20px; font-size: 13px; font-weight: 700; margin-bottom: 14px; }
.method-badge.self { background: var(--mm-green-light); color: var(--mm-green-dark); }
.method-badge.personal { background: #E3F2FD; color: var(--mm-blue); }
.basket-card { padding: 0; overflow: hidden; }
.basket-header { display: flex; justify-content: space-between; align-items: center; padding: 10px 14px; background: var(--mm-surface); border-left: 4px solid #333; }
.basket-store { font-weight: 700; font-size: 14px; }
.basket-retailer { font-size: 12px; color: var(--mm-text-secondary); }
.basket-item { display: flex; justify-content: space-between; align-items: flex-start; padding: 10px 14px; border-top: 1px solid var(--mm-border); }
.item-left { display: flex; flex-direction: column; gap: 2px; flex: 1; }
.item-name { font-size: 14px; font-weight: 500; }
.item-brand { font-size: 12px; }
.item-right { display: flex; flex-direction: column; align-items: flex-end; gap: 2px; }
.item-original { font-size: 11px; text-decoration: line-through; color: var(--mm-text-secondary); }
.item-price { font-size: 15px; font-weight: 700; }
.deal-chip { font-size: 10px; font-weight: 700; background: var(--mm-green); color: #fff; padding: 1px 6px; border-radius: 4px; }
.basket-footer { display: flex; justify-content: space-between; padding: 10px 14px; border-top: 1px solid var(--mm-border); background: var(--mm-surface); }
.fw-700 { font-weight: 700; }
.fw-600 { font-weight: 600; }
.cost-line { display: flex; justify-content: space-between; padding: 6px 0; font-size: 14px; }
.total-line { font-size: 17px; font-weight: 800; }
.field-label { font-size: 11px; font-weight: 700; color: var(--mm-text-secondary); text-transform: uppercase; margin-bottom: 6px; }
.delivery-summary { background: var(--mm-surface); }
.approval-note { text-align: center; font-size: 12px; color: var(--mm-text-secondary); margin-top: 10px; }
</style>
