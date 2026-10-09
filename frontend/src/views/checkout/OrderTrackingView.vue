<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { api, type OrderDto } from '../../services/api'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const order = ref<OrderDto | null>(null)
const loading = ref(true)
const error = ref('')

const statuses = [
  { key: 'Placed', icon: '📋', label: 'Order Placed' },
  { key: 'ShopperAssigned', icon: '🧑🛒', label: 'Shopper Assigned' },
  { key: 'ShoppingInProgress', icon: '🛒', label: 'Shopping in Progress' },
  { key: 'OutForDelivery', icon: '🚗', label: 'Out for Delivery' },
  { key: 'Delivered', icon: '✅', label: 'Delivered' },
]

const currentIndex = computed(() =>
  statuses.findIndex(s => s.key === order.value?.status)
)

onMounted(async () => {
  try {
    order.value = await api.getOrder(route.params.orderNumber as string)
  } catch {
    error.value = 'Order not found.'
  } finally {
    loading.value = false
  }
})

async function simulateNext() {
  if (!order.value) return
  const next = statuses[currentIndex.value + 1]
  if (!next) return
  order.value = await api.updateOrderStatus(order.value.id, next.key, `${next.label} [DEMO simulation]`)
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-ZA', { hour: '2-digit', minute: '2-digit' })
}
</script>

<template>
  <div class="page">
    <div class="co-header">
      <button class="back-btn" @click="router.push('/')">← Home</button>
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">Order Tracking</h1>
    </div>

    <div v-if="loading" class="loading"><div class="spinner" /> Loading order…</div>
    <div v-else-if="error" class="error-card">{{ error }}</div>

    <template v-else-if="order">
      <div class="order-num-bar">
        <span class="text-secondary">Order</span>
        <span class="order-num">{{ order.orderNumber }}</span>
      </div>

      <!-- Status timeline -->
      <div class="card mb-14">
        <div
          v-for="(s, i) in statuses"
          :key="s.key"
          class="status-step"
          :class="{
            done: i < currentIndex,
            current: i === currentIndex,
            pending: i > currentIndex
          }"
        >
          <div class="step-icon">{{ i <= currentIndex ? s.icon : '○' }}</div>
          <div class="step-body">
            <div class="step-label">{{ s.label }}</div>
            <div v-if="i === currentIndex" class="step-current-badge">Current</div>
          </div>
          <div v-if="i < statuses.length - 1" class="step-line" :class="{ filled: i < currentIndex }" />
        </div>
      </div>

      <!-- Status history -->
      <div class="card mb-14">
        <div class="field-label">Activity</div>
        <div v-for="event in [...order.statusHistory].reverse()" :key="event.timestamp" class="event-row">
          <span class="event-time">{{ formatTime(event.timestamp) }}</span>
          <span class="event-msg">{{ event.message }}</span>
        </div>
      </div>

      <!-- Delivery info -->
      <div v-if="order.delivery" class="card mb-14">
        <div class="field-label">Delivery Details</div>
        <div class="fw-600">{{ order.delivery.address }}</div>
        <div class="text-secondary">{{ order.delivery.contactNumber }}</div>
        <div class="text-secondary">Window: {{ order.delivery.deliveryWindow }}</div>
      </div>

      <!-- Demo advance button -->
      <div v-if="order.status !== 'Delivered' && order.status !== 'Cancelled'" class="demo-advance">
        <div class="demo-label">⚠️ DEMO — Simulate next status update:</div>
        <button class="btn btn-outline btn-sm" @click="simulateNext">
          Advance to: {{ statuses[currentIndex + 1]?.label ?? 'N/A' }}
        </button>
      </div>

      <div v-if="order.status === 'Delivered'" class="delivered-banner">
        ✅ Your order has been delivered! Enjoy your shopping.
      </div>
    </template>
  </div>
</template>

<style scoped>
.co-header { margin-bottom: 8px; }
.back-btn { background: none; border: none; color: var(--mm-green-dark); font-size: 15px; font-weight: 600; cursor: pointer; padding: 0; margin-bottom: 4px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; margin-bottom: 4px; }
.mb-14 { margin-bottom: 14px; }
.order-num-bar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 14px; }
.order-num { font-size: 16px; font-weight: 700; color: var(--mm-green-dark); }
.status-step { display: flex; align-items: flex-start; gap: 12px; padding: 10px 0; position: relative; }
.step-icon { font-size: 22px; width: 32px; text-align: center; flex-shrink: 0; }
.step-body { flex: 1; }
.step-label { font-size: 14px; font-weight: 600; }
.status-step.pending .step-label { color: var(--mm-text-secondary); font-weight: 400; }
.status-step.current .step-label { color: var(--mm-green-dark); }
.step-current-badge { display: inline-block; font-size: 11px; font-weight: 700; background: var(--mm-green); color: #fff; padding: 2px 8px; border-radius: 10px; margin-top: 3px; }
.step-line { position: absolute; left: 15px; top: 42px; width: 2px; height: 20px; background: var(--mm-border); }
.step-line.filled { background: var(--mm-green); }
.field-label { font-size: 11px; font-weight: 700; color: var(--mm-text-secondary); text-transform: uppercase; margin-bottom: 8px; }
.event-row { display: flex; gap: 10px; padding: 6px 0; border-bottom: 1px solid var(--mm-border); font-size: 13px; }
.event-row:last-child { border-bottom: none; }
.event-time { color: var(--mm-text-secondary); flex-shrink: 0; font-size: 12px; }
.event-msg { flex: 1; }
.fw-600 { font-weight: 600; }
.demo-advance { background: #FFF3E0; border-radius: var(--mm-radius-sm); padding: 12px 14px; margin-bottom: 14px; }
.demo-label { font-size: 12px; color: var(--mm-orange); margin-bottom: 8px; }
.delivered-banner { background: var(--mm-green-light); border: 2px solid var(--mm-green); border-radius: var(--mm-radius); padding: 16px; text-align: center; font-size: 16px; font-weight: 700; color: var(--mm-green-dark); }
.error-card { background: #FFEBEE; color: var(--mm-red); padding: 14px; border-radius: var(--mm-radius); font-size: 14px; }
</style>
