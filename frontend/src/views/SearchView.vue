<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { api, type ProductSearchResult, type StorePriceDto } from '../services/api'
import { useListStore } from '../stores/listStore'

const listStore = useListStore()
const query = ref('')
const results = ref<ProductSearchResult[]>([])
const loading = ref(false)
const error = ref('')

// Track selected store per product: productId -> storeId
const selectedStores = ref<Record<string, string>>({})

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

// Emoji product images keyed by category
const categoryImages: Record<string, string> = {
  dairy: '🥛', grains: '🌾', meat: '🍗', bakery: '🍞',
  toiletries: '🪥', cleaning: '🧺', baby: '👶', pantry: '🫙',
}

function productImage(p: ProductSearchResult) {
  return categoryImages[p.category] ?? '🛍️'
}

function selectedStore(p: ProductSearchResult): StorePriceDto | null {
  const sid = selectedStores.value[p.id]
  return p.storePrices.find(s => s.storeId === sid) ?? p.storePrices[0] ?? null
}

function selectStore(productId: string, storeId: string) {
  selectedStores.value[productId] = storeId
}

function effectivePrice(sp: StorePriceDto) {
  return sp.hasDeal ? sp.dealPrice ?? sp.price : sp.price
}

function isInList(id: string) {
  return listStore.items.some(i => i.productId === id)
}

function toggle(product: ProductSearchResult) {
  if (isInList(product.id)) listStore.removeItem(product.id)
  else listStore.addItem(product.id, product.name)
}

function isBestDeal(sp: StorePriceDto, storePrices: StorePriceDto[]) {
  const prices = storePrices.map(s => effectivePrice(s))
  return effectivePrice(sp) === Math.min(...prices)
}
</script>

<template>
  <div class="page">
    <h1 class="page-title">Search Products</h1>

    <div class="search-box mb-16">
      <span class="search-icon">🔍</span>
      <input v-model="query" placeholder="Search milk, rice, toothpaste…" autofocus />
    </div>

    <div v-if="loading" class="loading"><div class="spinner" /> Searching…</div>
    <div v-else-if="error" class="error-card">{{ error }}</div>
    <div v-else-if="results.length === 0 && query" class="empty-state">No products found for "{{ query }}"</div>
    <div v-else-if="results.length === 0" class="hint-text">
      Try: milk, bread, chicken, rice, toothpaste, eggs, washing powder…
    </div>

    <div v-else class="results-list">
      <div v-for="p in results" :key="p.id" class="product-card card">

        <!-- Top row: image + name + add button -->
        <div class="product-top">
          <div class="product-img">{{ productImage(p) }}</div>
          <div class="product-info">
            <div class="product-name">{{ p.name }}</div>
            <div class="product-meta">{{ p.brand }} · {{ p.unit }}</div>
          </div>
          <button
            class="btn btn-sm"
            :class="isInList(p.id) ? 'btn-outline' : 'btn-primary'"
            @click="toggle(p)"
          >{{ isInList(p.id) ? '✓ Added' : '+ Add' }}</button>
        </div>

        <!-- Selected store price display -->
        <div class="selected-price-row" v-if="p.storePrices.length && selectedStore(p)">
          <div class="selected-price-info">
            <span class="from-label">from</span>
            <span class="selected-price">R{{ effectivePrice(selectedStore(p)!).toFixed(2) }}</span>
            <span v-if="selectedStore(p)!.hasDeal" class="was-price">R{{ selectedStore(p)!.price.toFixed(2) }}</span>
          </div>
          <span class="selected-store-name">{{ selectedStore(p)!.storeName }}</span>
        </div>

        <!-- Store chips — clickable to select preferred store -->
        <div class="store-chips" v-if="p.storePrices.length">
          <button
            v-for="sp in p.storePrices"
            :key="sp.storeId"
            class="store-chip"
            :class="{
              deal: sp.hasDeal,
              selected: (selectedStores[p.id] ?? p.storePrices[0]?.storeId) === sp.storeId,
              best: isBestDeal(sp, p.storePrices)
            }"
            @click="selectStore(p.id, sp.storeId)"
          >
            <span class="chip-retailer">{{ sp.retailerName }}</span>
            <span class="chip-price">R{{ effectivePrice(sp).toFixed(2) }}</span>
            <span v-if="sp.hasDeal" class="chip-deal-tag">DEAL</span>
            <span v-else-if="isBestDeal(sp, p.storePrices)" class="chip-best-tag">BEST</span>
          </button>
        </div>

      </div>
    </div>
  </div>
</template>

<style scoped>
.page-title { font-size: 24px; font-weight: 800; margin-bottom: 16px; }
.mb-16 { margin-bottom: 16px; }
.search-box { position: relative; display: flex; align-items: center; }
.search-icon { position: absolute; left: 14px; font-size: 18px; pointer-events: none; }
.search-box input { padding-left: 44px; }

.results-list { display: flex; flex-direction: column; gap: 12px; }

.product-card { padding: 16px; }

/* Top row */
.product-top { display: flex; align-items: center; gap: 12px; margin-bottom: 12px; }
.product-img {
  width: 52px; height: 52px;
  border-radius: 12px;
  background: var(--mm-surface);
  display: flex; align-items: center; justify-content: center;
  font-size: 28px;
  flex-shrink: 0;
}
.product-info { flex: 1; min-width: 0; }
.product-name { font-size: 15px; font-weight: 700; margin-bottom: 2px; }
.product-meta { font-size: 12px; color: var(--mm-text-secondary); }

/* Selected price row */
.selected-price-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--mm-surface);
  border-radius: var(--mm-radius-sm);
  padding: 8px 12px;
  margin-bottom: 10px;
}
.selected-price-info { display: flex; align-items: baseline; gap: 6px; }
.from-label { font-size: 12px; color: var(--mm-text-secondary); }
.selected-price { font-size: 20px; font-weight: 800; color: var(--mm-green-dark); }
.was-price { font-size: 13px; color: var(--mm-text-secondary); text-decoration: line-through; }
.selected-store-name { font-size: 13px; font-weight: 600; color: var(--mm-text-secondary); }

/* Store chips */
.store-chips { display: flex; flex-wrap: wrap; gap: 7px; }
.store-chip {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 6px 11px;
  border-radius: 20px;
  border: 2px solid var(--mm-border);
  background: var(--mm-card);
  cursor: pointer;
  transition: all 0.15s;
  font-size: 12px;
}
.store-chip:hover { border-color: #aaa; }
.store-chip.selected { border-color: var(--mm-green); background: var(--mm-green-light); }
.store-chip.deal { border-color: var(--mm-green); }
.chip-retailer { font-weight: 600; color: var(--mm-text); }
.chip-price { font-weight: 700; color: var(--mm-text); }
.store-chip.selected .chip-retailer,
.store-chip.selected .chip-price { color: var(--mm-green-dark); }
.chip-deal-tag {
  font-size: 9px; font-weight: 800;
  background: var(--mm-green); color: #fff;
  padding: 1px 5px; border-radius: 4px;
  text-transform: uppercase;
}
.chip-best-tag {
  font-size: 9px; font-weight: 800;
  background: var(--mm-orange); color: #fff;
  padding: 1px 5px; border-radius: 4px;
  text-transform: uppercase;
}

.error-card { background: #FFEBEE; color: var(--mm-red); padding: 14px; border-radius: var(--mm-radius); font-size: 14px; }
.empty-state, .hint-text { text-align: center; padding: 48px 20px; color: var(--mm-text-secondary); font-size: 15px; line-height: 1.8; }
</style>
