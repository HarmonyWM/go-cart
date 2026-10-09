<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api, type OptimisationResult, type ShoppingOptionDto } from '../services/api'
import { useListStore } from '../stores/listStore'
import { usePrefsStore } from '../stores/prefsStore'
import { useRouter } from 'vue-router'

const listStore = useListStore()
const prefsStore = usePrefsStore()
const router = useRouter()

const result = ref<OptimisationResult | null>(null)
const loading = ref(false)
const error = ref('')
const expanded = ref<string | null>(null)

onMounted(async () => {
  if (!listStore.items.length) { router.push('/list'); return }
  await prefsStore.load()
  await optimise()
})

async function optimise() {
  loading.value = true
  error.value = ''
  result.value = null
  try {
    result.value = await api.optimise(
      listStore.items,
      listStore.budget,
      listStore.priority,
      prefsStore.prefs
    )
  } catch {
    error.value = 'Could not reach the server. Is the backend running?'
  } finally {
    loading.value = false
  }
}

function budgetClass(status: string) {
  return status === 'under' ? 'badge-green' : status === 'close' ? 'badge-orange' : 'badge-red'
}

function budgetLabel(status: string) {
  return status === 'under' ? '🟢 Under budget' : status === 'close' ? '🟠 Close to budget' : '🔴 Over budget'
}

function toggleExpand(key: string) {
  expanded.value = expanded.value === key ? null : key
}

const priorityLabels: Record<string, string> = {
  saveMoney: '💰 Save Money',
  saveTravel: '🚗 Save Travel',
  saveTime: '⏱️ Save Time',
  supportLocal: '🏪 Support Local',
  bestOverall: '⚖️ Best Overall',
}
</script>

<template>
  <div class="page">
    <div class="opt-header">
      <button class="back-btn" @click="router.push('/list')">← Back</button>
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">Shopping Options</h1>
      <div class="priority-tag">{{ priorityLabels[listStore.priority] }}</div>
    </div>

    <div v-if="loading" class="loading"><div class="spinner" /> Optimising your shopping…</div>

    <div v-else-if="error" class="error-card">{{ error }}</div>

    <template v-else-if="result">
      <!-- Budget status -->
      <div v-if="result.budget" class="row-between mb-12">
        <span class="text-secondary">Budget: R{{ result.budget }}</span>
        <span class="badge" :class="budgetClass(result.budgetStatus)">
          {{ budgetLabel(result.budgetStatus) }}
        </span>
      </div>

      <!-- Recommended -->
      <div class="recommended-card card mb-16">
        <div class="rec-label">⚖️ MaliMove Recommends</div>
        <div class="rec-store">{{ result.recommended.label }}</div>
        <div class="rec-reason">{{ result.recommendationReason }}</div>
        <div class="cost-row">
          <div class="cost-item">
            <div class="cost-val">R{{ result.recommended.productTotal.toFixed(2) }}</div>
            <div class="cost-label">Products</div>
          </div>
          <div class="cost-sep">+</div>
          <div class="cost-item">
            <div class="cost-val">R{{ result.recommended.travelCost.toFixed(2) }}</div>
            <div class="cost-label">Est. Travel</div>
          </div>
          <div class="cost-sep">=</div>
          <div class="cost-item total">
            <div class="cost-val">R{{ result.recommended.totalEstimatedCost.toFixed(2) }}</div>
            <div class="cost-label">Total</div>
          </div>
        </div>
        <div class="rec-meta">
          <span>🏪 {{ result.recommended.storeCount }} store{{ result.recommended.storeCount > 1 ? 's' : '' }}</span>
          <span>⏱️ ~{{ result.recommended.estimatedTotalMinutes }} min</span>
          <span>📍 {{ result.recommended.totalDistanceKm.toFixed(1) }} km</span>
        </div>
      </div>

      <!-- All options -->
      <div class="section-title">All Options</div>
      <div class="options-list">
        <div
          v-for="opt in result.options"
          :key="opt.key"
          class="option-card card"
          :class="{ 'is-recommended': opt.key === result.recommended.key }"
        >
          <div class="option-header" @click="toggleExpand(opt.key)">
            <div class="option-left">
              <span class="option-icon">{{ opt.icon }}</span>
              <div>
                <div class="option-label">{{ opt.label }}</div>
                <div class="option-desc text-secondary">{{ opt.description }}</div>
              </div>
            </div>
            <div class="option-right">
              <div class="option-total">R{{ opt.totalEstimatedCost.toFixed(2) }}</div>
              <div v-if="opt.estimatedSavings > 0" class="option-saving text-green">
                Save R{{ opt.estimatedSavings.toFixed(2) }}
              </div>
              <span class="expand-icon">{{ expanded === opt.key ? '▲' : '▼' }}</span>
            </div>
          </div>

          <div v-if="expanded === opt.key" class="option-detail">
            <div class="divider" />
            <div class="detail-row">
              <span class="text-secondary">Products</span>
              <span>R{{ opt.productTotal.toFixed(2) }}</span>
            </div>
            <div class="detail-row">
              <span class="text-secondary">Est. Travel</span>
              <span>R{{ opt.travelCost.toFixed(2) }}</span>
            </div>
            <div class="detail-row">
              <span class="text-secondary">Stores</span>
              <span>{{ opt.storeCount }}</span>
            </div>
            <div class="detail-row">
              <span class="text-secondary">Est. Time</span>
              <span>~{{ opt.estimatedTotalMinutes }} min</span>
            </div>
            <div class="detail-row">
              <span class="text-secondary">Distance</span>
              <span>{{ opt.totalDistanceKm.toFixed(1) }} km</span>
            </div>

            <div v-for="basket in opt.storeBaskets" :key="basket.storeId" class="store-basket">
              <div class="basket-header" :style="{ borderLeftColor: basket.color }">
                <span class="basket-store">{{ basket.storeName }}</span>
                <span class="basket-subtotal">R{{ basket.subTotal.toFixed(2) }}</span>
              </div>
              <div v-for="item in basket.items" :key="item.productId" class="basket-item">
                <span>{{ item.productName }} × {{ item.quantity }}</span>
                <span>
                  <span v-if="item.hasDeal" class="deal-strike">R{{ item.originalPrice?.toFixed(2) }}</span>
                  R{{ item.lineTotal.toFixed(2) }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <button class="btn btn-outline btn-full mt-16" @click="optimise">🔄 Re-optimise</button>
    </template>
  </div>
</template>

<style scoped>
.opt-header { margin-bottom: 16px; }
.back-btn { background: none; border: none; color: var(--mm-green-dark); font-size: 15px; font-weight: 600; cursor: pointer; padding: 0; margin-bottom: 4px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; }
.priority-tag { font-size: 13px; color: var(--mm-text-secondary); margin-top: 2px; }
.mb-12 { margin-bottom: 12px; }
.mb-16 { margin-bottom: 16px; }
.mt-16 { margin-top: 16px; }
.row-between { display: flex; justify-content: space-between; align-items: center; }
.recommended-card { background: var(--mm-green-light); border: 2px solid var(--mm-green); }
.rec-label { font-size: 12px; font-weight: 700; color: var(--mm-green-dark); text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 4px; }
.rec-store { font-size: 20px; font-weight: 800; margin-bottom: 8px; }
.rec-reason { font-size: 14px; color: var(--mm-text); margin-bottom: 14px; line-height: 1.5; }
.cost-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; }
.cost-item { text-align: center; flex: 1; }
.cost-item.total { background: var(--mm-green); border-radius: var(--mm-radius-sm); padding: 8px 4px; color: #fff; }
.cost-val { font-size: 16px; font-weight: 700; }
.cost-label { font-size: 11px; opacity: 0.8; }
.cost-sep { font-size: 18px; font-weight: 700; color: var(--mm-text-secondary); }
.rec-meta { display: flex; gap: 12px; font-size: 13px; color: var(--mm-text-secondary); }
.options-list { display: flex; flex-direction: column; gap: 10px; }
.option-card { cursor: pointer; }
.option-card.is-recommended { border: 2px solid var(--mm-green); }
.option-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; }
.option-left { display: flex; gap: 10px; align-items: flex-start; flex: 1; }
.option-icon { font-size: 24px; flex-shrink: 0; }
.option-label { font-size: 15px; font-weight: 600; }
.option-desc { font-size: 12px; margin-top: 2px; }
.option-right { text-align: right; flex-shrink: 0; }
.option-total { font-size: 18px; font-weight: 700; }
.option-saving { font-size: 12px; font-weight: 600; }
.expand-icon { font-size: 11px; color: var(--mm-text-secondary); }
.option-detail { margin-top: 4px; }
.detail-row { display: flex; justify-content: space-between; padding: 6px 0; font-size: 14px; border-bottom: 1px solid var(--mm-border); }
.detail-row:last-of-type { border-bottom: none; }
.store-basket { margin-top: 10px; border-radius: var(--mm-radius-sm); overflow: hidden; border: 1px solid var(--mm-border); }
.basket-header { display: flex; justify-content: space-between; padding: 8px 12px; background: var(--mm-surface); border-left: 4px solid #333; }
.basket-store { font-weight: 600; font-size: 14px; }
.basket-subtotal { font-weight: 700; font-size: 14px; }
.basket-item { display: flex; justify-content: space-between; padding: 6px 12px; font-size: 13px; border-top: 1px solid var(--mm-border); }
.deal-strike { text-decoration: line-through; color: var(--mm-text-secondary); margin-right: 4px; font-size: 12px; }
.error-card { background: #FFEBEE; color: var(--mm-red); padding: 14px; border-radius: var(--mm-radius); font-size: 14px; }
</style>
