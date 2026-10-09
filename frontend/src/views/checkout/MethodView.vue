<script setup lang="ts">
import { useCheckoutStore } from '../../stores/checkoutStore'
import { useRouter } from 'vue-router'

const checkout = useCheckoutStore()
const router = useRouter()

if (!checkout.selectedOption) router.push('/optimise')

function select(m: 'ShopMyself' | 'PersonalShopper') {
  checkout.method = m
  if (m === 'ShopMyself') router.push('/checkout/review')
  else router.push('/checkout/delivery')
}
</script>

<template>
  <div class="page">
    <div class="co-header">
      <button class="back-btn" @click="router.push('/optimise')">← Back</button>
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">How would you like to shop?</h1>
    </div>

    <div class="step-indicator">Step 1 of 4</div>

    <div class="method-card card" @click="select('ShopMyself')">
      <div class="method-icon">🛒</div>
      <div class="method-body">
        <div class="method-title">Shop Myself</div>
        <div class="method-desc">We'll give you the optimised shopping list and recommended stores. No extra fees.</div>
        <div class="method-tag free">Free</div>
      </div>
      <div class="method-arrow">›</div>
    </div>

    <div class="method-card card" @click="select('PersonalShopper')">
      <div class="method-icon">🧑‍🛒</div>
      <div class="method-body">
        <div class="method-title">MaliMove Shops For Me</div>
        <div class="method-desc">A personal shopper will collect your items and deliver to your door.</div>
        <div class="fees-row">
          <span class="method-tag">Delivery R{{ checkout.DELIVERY_FEE.toFixed(2) }}</span>
          <span class="method-tag">Service R{{ checkout.SERVICE_FEE.toFixed(2) }}</span>
        </div>
        <div class="demo-notice">⚠️ PROTOTYPE — Shopper assignment is simulated</div>
      </div>
      <div class="method-arrow">›</div>
    </div>
  </div>
</template>

<style scoped>
.co-header { margin-bottom: 8px; }
.back-btn { background: none; border: none; color: var(--mm-green-dark); font-size: 15px; font-weight: 600; cursor: pointer; padding: 0; margin-bottom: 4px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; margin-bottom: 4px; }
.step-indicator { font-size: 12px; color: var(--mm-text-secondary); margin-bottom: 20px; }
.method-card { display: flex; align-items: flex-start; gap: 14px; padding: 18px; margin-bottom: 14px; cursor: pointer; transition: border-color 0.15s; border: 2px solid var(--mm-border); }
.method-card:hover { border-color: var(--mm-green); }
.method-icon { font-size: 36px; flex-shrink: 0; }
.method-body { flex: 1; }
.method-title { font-size: 17px; font-weight: 700; margin-bottom: 6px; }
.method-desc { font-size: 14px; color: var(--mm-text-secondary); margin-bottom: 8px; line-height: 1.5; }
.fees-row { display: flex; gap: 8px; flex-wrap: wrap; margin-bottom: 6px; }
.method-tag { display: inline-block; font-size: 12px; font-weight: 600; padding: 3px 10px; border-radius: 20px; background: var(--mm-green-light); color: var(--mm-green-dark); }
.method-tag.free { background: var(--mm-green); color: #fff; }
.demo-notice { font-size: 11px; color: var(--mm-orange); margin-top: 4px; }
.method-arrow { font-size: 24px; color: var(--mm-text-secondary); align-self: center; }
</style>
