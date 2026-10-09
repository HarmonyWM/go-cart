<script setup lang="ts">
import { ref, watch } from 'vue'
import { api, type ProductSearchResult } from '../services/api'
import { useListStore } from '../stores/listStore'

const listStore = useListStore()
const query = ref('')
const results = ref<ProductSearchResult[]>([])
const loading = ref(false)
const error = ref('')

let debounce: ReturnType<typeof setTimeout>
watch(query, (val) => {
  clearTimeout(debounce)
  if (!val.trim()) { results.value = []; return }
  debounce = setTimeout(async () => {
    loading.value = true
    error.value = ''
    try { results.value = await api.searchProducts(val) }
    catch { error.value = 'Could not reach the server. Is the backend running?' }
    finally { loading.value = false }
  }, 350)
})

function isInList(id: string) {
  return listStore.items.some(i => i.productId === id)
}

function toggle(product: ProductSearchResult) {
  if (isInList(product.id)) listStore.removeItem(product.id)
  else listStore.addItem(product.id, product.name)
}

function effectivePrice(p: ProductSearchResult) {
  const best = p.storePrices[0]
  if (!best) return null
  return best.hasDeal ? best.dealPrice : best.price
}
</script>

<template>
  <div class="page">
    <div class="search-header">
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">Search Products</h1>
    </div>

    <div class="search-box mb-16">
      <span class="search-icon">🔍</span>
      <input v-model="query" placeholder="Search milk, rice, toothpaste…" autofocus />
    </div>

    <div v-if="loading" class="loading"><div class="spinner" /> Searching…</div>

    <div v-else-if="error" class="error-card">{{ error }}</div>

    <div v-else-if="results.length === 0 && query" class="empty-state">
      No products found for "{{ query }}"
    </div>

    <div v-else-if="results.length === 0" class="hint-text">
      Try searching for: milk, bread, chicken, toothpaste…
    </div>

    <div v-else class="results-list">
      <div v-for="p in results" :key="p.id" class="product-card card">
        <div class="product-main">
          <div class="product-info">
            <div class="product-name">{{ p.name }}</div>
            <div class="product-meta">{{ p.brand }} · {{ p.unit }}</div>
            <div class="store-prices" v-if="p.storePrices.length">
              <span
                v-for="sp in p.storePrices.slice(0, 3)"
                :key="sp.storeId"
                class="store-price-chip"
                :class="{ deal: sp.hasDeal }"
              >
                {{ sp.retailerName }}
                <strong>R{{ (sp.hasDeal ? sp.dealPrice : sp.price)?.toFixed(2) }}</strong>
                <span v-if="sp.hasDeal" class="deal-tag">DEAL</span>
              </span>
            </div>
          </div>
          <div class="product-right">
            <div class="product-price" v-if="effectivePrice(p) !== null">
              R{{ effectivePrice(p)?.toFixed(2) }}
            </div>
            <div class="product-price-label text-secondary">from</div>
            <button
              class="btn btn-sm"
              :class="isInList(p.id) ? 'btn-outline' : 'btn-primary'"
              @click="toggle(p)"
            >
              {{ isInList(p.id) ? '✓ Added' : '+ Add' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.search-header { margin-bottom: 16px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; }
.mb-16 { margin-bottom: 16px; }
.search-box {
  position: relative;
  display: flex;
  align-items: center;
}
.search-icon {
  position: absolute;
  left: 14px;
  font-size: 18px;
  pointer-events: none;
}
.search-box input { padding-left: 42px; }
.results-list { display: flex; flex-direction: column; gap: 10px; }
.product-card { padding: 14px; }
.product-main { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; }
.product-info { flex: 1; min-width: 0; }
.product-name { font-size: 15px; font-weight: 600; margin-bottom: 2px; }
.product-meta { font-size: 12px; color: var(--mm-text-secondary); margin-bottom: 8px; }
.store-prices { display: flex; flex-wrap: wrap; gap: 6px; }
.store-price-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  padding: 3px 8px;
  border-radius: 6px;
  background: var(--mm-surface);
  color: var(--mm-text-secondary);
}
.store-price-chip.deal { background: var(--mm-green-light); color: var(--mm-green-dark); }
.deal-tag {
  font-size: 10px;
  font-weight: 700;
  background: var(--mm-green);
  color: #fff;
  padding: 1px 4px;
  border-radius: 4px;
}
.product-right { display: flex; flex-direction: column; align-items: flex-end; gap: 4px; flex-shrink: 0; }
.product-price { font-size: 18px; font-weight: 700; color: var(--mm-green-dark); }
.product-price-label { font-size: 11px; }
.error-card {
  background: #FFEBEE;
  color: var(--mm-red);
  padding: 14px;
  border-radius: var(--mm-radius);
  font-size: 14px;
}
.empty-state, .hint-text {
  text-align: center;
  padding: 40px 20px;
  color: var(--mm-text-secondary);
  font-size: 15px;
}
</style>
