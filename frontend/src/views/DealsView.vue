<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { api, type DealDto } from '../services/api'
import { useListStore } from '../stores/listStore'

const listStore = useListStore()
const deals = ref<DealDto[]>([])
const loading = ref(true)
const search = ref('')
const selectedStore = ref('')
const selectedCategory = ref('')
const matchingOnly = ref(false)

const stores = computed(() => [...new Set(deals.value.map(d => d.storeName))])
const categories = computed(() => [...new Set(deals.value.map(d => d.category))])
const listProductIds = computed(() => listStore.items.map(i => i.productId))

const filtered = computed(() => {
  let d = deals.value
  if (matchingOnly.value) d = d.filter(x => listProductIds.value.includes(x.productId))
  if (selectedStore.value) d = d.filter(x => x.storeName === selectedStore.value)
  if (selectedCategory.value) d = d.filter(x => x.category === selectedCategory.value)
  if (search.value) d = d.filter(x => x.title.toLowerCase().includes(search.value.toLowerCase()))
  return d
})

const matchingCount = computed(() =>
  deals.value.filter(d => listProductIds.value.includes(d.productId)).length
)

onMounted(async () => {
  try { deals.value = await api.getDeals() }
  catch {}
  finally { loading.value = false }
})

// Emoji images per category
const categoryImages: Record<string, string> = {
  dairy: '🥛', grains: '🌾', meat: '🍗', bakery: '🍞',
  toiletries: '🪥', cleaning: '🧺', baby: '👶', pantry: '🫙',
}
function dealImage(d: DealDto) {
  return categoryImages[d.category] ?? '🏷️'
}

// Retailer brand colours
const retailerColors: Record<string, string> = {
  'Shoprite': '#E31837', 'Checkers': '#E31837',
  'Pick n Pay': '#E31837', 'SPAR': '#007A3D',
  'Woolworths': '#1A1A2E', 'Fresh Corner': '#F59E0B',
}
function retailerColor(name: string) {
  return retailerColors[name] ?? '#6B7280'
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-ZA', { day: 'numeric', month: 'short', year: 'numeric' })
}

function daysRemaining(iso: string) {
  const diff = new Date(iso).getTime() - Date.now()
  return Math.max(0, Math.ceil(diff / 86400000))
}

function daysLabel(iso: string) {
  const d = daysRemaining(iso)
  if (d === 0) return { text: 'Ends today!', urgent: true }
  if (d <= 3) return { text: `${d} day${d > 1 ? 's' : ''} left`, urgent: true }
  return { text: `${d} days left`, urgent: false }
}

function addToList(deal: DealDto) {
  listStore.addItem(deal.productId, deal.productName)
}

function isInList(productId: string) {
  return listStore.items.some(i => i.productId === productId)
}
</script>

<template>
  <div class="page">
    <h1 class="page-title">Deals 🔥</h1>

    <!-- Matching list banner -->
    <div v-if="matchingCount > 0" class="matching-banner" @click="matchingOnly = !matchingOnly">
      <span>🎯 {{ matchingCount }} deal{{ matchingCount > 1 ? 's' : '' }} match your list</span>
      <span class="badge" :class="matchingOnly ? 'badge-green' : 'badge-blue'">
        {{ matchingOnly ? 'Showing matches' : 'Show matches' }}
      </span>
    </div>

    <!-- Search -->
    <div class="search-box mb-12">
      <span class="search-icon">🔍</span>
      <input v-model="search" placeholder="Search deals…" />
    </div>

    <!-- Store filter -->
    <div class="filter-row mb-10">
      <button class="chip" :class="{ active: !selectedStore }" @click="selectedStore = ''">All Stores</button>
      <button
        v-for="s in stores" :key="s"
        class="chip" :class="{ active: selectedStore === s }"
        @click="selectedStore = selectedStore === s ? '' : s"
      >{{ s }}</button>
    </div>

    <!-- Category filter -->
    <div class="filter-row mb-16">
      <button class="chip" :class="{ active: !selectedCategory }" @click="selectedCategory = ''">All</button>
      <button
        v-for="c in categories" :key="c"
        class="chip" :class="{ active: selectedCategory === c }"
        @click="selectedCategory = selectedCategory === c ? '' : c"
      >{{ c }}</button>
    </div>

    <div v-if="loading" class="loading"><div class="spinner" /> Loading deals…</div>
    <div v-else-if="filtered.length === 0" class="empty-state">No deals found</div>

    <div v-else class="deals-grid">
      <div v-for="deal in filtered" :key="deal.id" class="deal-card card">

        <!-- Image + discount badge -->
        <div class="deal-image-row">
          <div class="deal-img">{{ dealImage(deal) }}</div>
          <div class="deal-badges">
            <span class="discount-badge">-{{ deal.discountPercent }}%</span>
            <span v-if="daysLabel(deal.validUntil).urgent" class="urgent-badge">
              🔥 {{ daysLabel(deal.validUntil).text }}
            </span>
          </div>
        </div>

        <!-- Product name -->
        <div class="deal-name">{{ deal.title }}</div>

        <!-- Store info -->
        <div class="store-info-row">
          <span class="store-dot" :style="{ background: retailerColor(deal.retailerName) }" />
          <span class="store-name-text">{{ deal.storeName }}</span>
          <span class="retailer-name-text">· {{ deal.retailerName }}</span>
        </div>

        <!-- Prices -->
        <div class="deal-prices">
          <span class="deal-price">R{{ deal.dealPrice.toFixed(2) }}</span>
          <span class="original-price">R{{ deal.originalPrice.toFixed(2) }}</span>
          <span class="saving-chip">Save R{{ deal.savingsAmount.toFixed(2) }}</span>
        </div>

        <!-- Date range -->
        <div class="date-range">
          <span class="date-item">
            <span class="date-label">From</span>
            <span class="date-val">{{ formatDate(deal.validUntil) }}</span>
          </span>
          <span class="date-sep">→</span>
          <span class="date-item">
            <span class="date-label">Until</span>
            <span class="date-val">{{ formatDate(deal.validUntil) }}</span>
          </span>
          <span class="days-pill" :class="{ urgent: daysLabel(deal.validUntil).urgent }">
            {{ daysLabel(deal.validUntil).text }}
          </span>
        </div>

        <!-- Action -->
        <button
          class="btn btn-sm btn-full"
          :class="isInList(deal.productId) ? 'btn-outline' : 'btn-primary'"
          @click="addToList(deal)"
        >
          {{ isInList(deal.productId) ? '✓ In My List' : '+ Add to List' }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-title { font-size: 24px; font-weight: 800; margin-bottom: 16px; }
.mb-10 { margin-bottom: 10px; }
.mb-12 { margin-bottom: 12px; }
.mb-16 { margin-bottom: 16px; }

.matching-banner {
  display: flex; justify-content: space-between; align-items: center;
  background: var(--mm-green-light); border: 1px solid var(--mm-green);
  border-radius: var(--mm-radius-sm); padding: 10px 14px;
  margin-bottom: 12px; cursor: pointer; font-size: 14px; font-weight: 500;
}
.search-box { position: relative; display: flex; align-items: center; }
.search-icon { position: absolute; left: 14px; font-size: 18px; pointer-events: none; }
.search-box input { padding-left: 44px; }
.filter-row { display: flex; gap: 8px; overflow-x: auto; padding-bottom: 4px; }
.filter-row::-webkit-scrollbar { display: none; }

/* Grid: 1 col mobile, 2 col tablet+, 3 col desktop */
.deals-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 14px;
}
@media (min-width: 600px) { .deals-grid { grid-template-columns: 1fr 1fr; } }
@media (min-width: 1000px) { .deals-grid { grid-template-columns: 1fr 1fr 1fr; } }

.deal-card { padding: 16px; display: flex; flex-direction: column; gap: 10px; }

/* Image row */
.deal-image-row { display: flex; justify-content: space-between; align-items: flex-start; }
.deal-img {
  width: 64px; height: 64px;
  border-radius: 14px;
  background: var(--mm-surface);
  display: flex; align-items: center; justify-content: center;
  font-size: 36px;
  flex-shrink: 0;
}
.deal-badges { display: flex; flex-direction: column; align-items: flex-end; gap: 5px; }
.discount-badge {
  background: var(--mm-orange); color: #fff;
  font-size: 14px; font-weight: 800;
  padding: 4px 12px; border-radius: 20px;
}
.urgent-badge {
  background: #FFEBEE; color: var(--mm-red);
  font-size: 11px; font-weight: 700;
  padding: 3px 8px; border-radius: 20px;
}

/* Product name */
.deal-name { font-size: 15px; font-weight: 700; line-height: 1.3; }

/* Store info */
.store-info-row { display: flex; align-items: center; gap: 6px; }
.store-dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.store-name-text { font-size: 13px; font-weight: 700; }
.retailer-name-text { font-size: 12px; color: var(--mm-text-secondary); }

/* Prices */
.deal-prices { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.deal-price { font-size: 24px; font-weight: 800; color: var(--mm-green-dark); }
.original-price { font-size: 14px; color: var(--mm-text-secondary); text-decoration: line-through; }
.saving-chip {
  font-size: 12px; font-weight: 700;
  background: var(--mm-green-light); color: var(--mm-green-dark);
  padding: 3px 10px; border-radius: 20px;
  margin-left: auto;
}

/* Date range */
.date-range {
  display: flex; align-items: center; gap: 8px;
  background: var(--mm-surface); border-radius: var(--mm-radius-sm);
  padding: 8px 12px; flex-wrap: wrap;
}
.date-item { display: flex; flex-direction: column; }
.date-label { font-size: 10px; color: var(--mm-text-secondary); text-transform: uppercase; font-weight: 600; }
.date-val { font-size: 13px; font-weight: 600; }
.date-sep { color: var(--mm-text-secondary); font-size: 14px; }
.days-pill {
  margin-left: auto;
  font-size: 11px; font-weight: 700;
  background: var(--mm-green-light); color: var(--mm-green-dark);
  padding: 3px 10px; border-radius: 20px;
}
.days-pill.urgent { background: #FFEBEE; color: var(--mm-red); }

.empty-state { text-align: center; padding: 48px 20px; color: var(--mm-text-secondary); }
</style>
