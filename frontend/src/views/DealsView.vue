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

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-ZA', { day: 'numeric', month: 'short' })
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
    <div class="deals-header">
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">Deals 🔥</h1>
    </div>

    <!-- Matching list banner -->
    <div v-if="matchingCount > 0" class="matching-banner" @click="matchingOnly = !matchingOnly">
      <span>🎯 {{ matchingCount }} deal{{ matchingCount > 1 ? 's' : '' }} match your shopping list</span>
      <span class="badge" :class="matchingOnly ? 'badge-green' : 'badge-blue'">
        {{ matchingOnly ? 'Showing matches' : 'Show matches' }}
      </span>
    </div>

    <!-- Search -->
    <div class="search-box mb-12">
      <span class="search-icon">🔍</span>
      <input v-model="search" placeholder="Search deals…" />
    </div>

    <!-- Filters -->
    <div class="filter-row mb-16">
      <button class="chip" :class="{ active: !selectedStore }" @click="selectedStore = ''">All Stores</button>
      <button
        v-for="s in stores" :key="s"
        class="chip" :class="{ active: selectedStore === s }"
        @click="selectedStore = selectedStore === s ? '' : s"
      >{{ s }}</button>
    </div>

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
        <div class="deal-badge-row">
          <span class="discount-badge">-{{ deal.discountPercent }}%</span>
          <span class="retailer-tag">{{ deal.retailerName }}</span>
        </div>
        <div class="deal-name">{{ deal.title }}</div>
        <div class="deal-prices">
          <span class="deal-price">R{{ deal.dealPrice.toFixed(2) }}</span>
          <span class="original-price">R{{ deal.originalPrice.toFixed(2) }}</span>
        </div>
        <div class="deal-saving text-green">Save R{{ deal.savingsAmount.toFixed(2) }}</div>
        <div class="deal-footer">
          <span class="text-secondary" style="font-size:12px">Valid until {{ formatDate(deal.validUntil) }}</span>
          <button
            class="btn btn-sm"
            :class="isInList(deal.productId) ? 'btn-outline' : 'btn-primary'"
            @click="addToList(deal)"
          >
            {{ isInList(deal.productId) ? '✓ In List' : '+ Add to List' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.deals-header { margin-bottom: 16px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; }
.mb-12 { margin-bottom: 12px; }
.mb-16 { margin-bottom: 16px; }
.matching-banner {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--mm-green-light);
  border: 1px solid var(--mm-green);
  border-radius: var(--mm-radius-sm);
  padding: 10px 14px;
  margin-bottom: 12px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
}
.search-box { position: relative; display: flex; align-items: center; }
.search-icon { position: absolute; left: 14px; font-size: 18px; pointer-events: none; }
.search-box input { padding-left: 42px; }
.filter-row { display: flex; gap: 8px; overflow-x: auto; padding-bottom: 4px; }
.filter-row::-webkit-scrollbar { display: none; }
.deals-grid { display: flex; flex-direction: column; gap: 12px; }
.deal-card { padding: 14px; }
.deal-badge-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.discount-badge {
  background: var(--mm-orange);
  color: #fff;
  font-size: 13px;
  font-weight: 700;
  padding: 3px 10px;
  border-radius: 20px;
}
.retailer-tag {
  font-size: 12px;
  font-weight: 600;
  color: var(--mm-text-secondary);
  background: var(--mm-surface);
  padding: 3px 10px;
  border-radius: 20px;
}
.deal-name { font-size: 15px; font-weight: 600; margin-bottom: 8px; }
.deal-prices { display: flex; align-items: baseline; gap: 10px; margin-bottom: 4px; }
.deal-price { font-size: 22px; font-weight: 800; color: var(--mm-green-dark); }
.original-price { font-size: 14px; color: var(--mm-text-secondary); text-decoration: line-through; }
.deal-saving { font-size: 13px; font-weight: 600; margin-bottom: 10px; }
.deal-footer { display: flex; justify-content: space-between; align-items: center; }
.empty-state { text-align: center; padding: 40px 20px; color: var(--mm-text-secondary); }
</style>
