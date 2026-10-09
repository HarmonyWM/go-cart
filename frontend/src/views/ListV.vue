<script setup lang="ts">
import { useListStore } from '../stores/listStore'
import { useRouter } from 'vue-router'

const listStore = useListStore()
const router = useRouter()

const priorities = [
  { key: 'saveMoney', icon: '💰', label: 'Save Money' },
  { key: 'saveTravel', icon: '🚗', label: 'Save Travel' },
  { key: 'saveTime', icon: '⏱️', label: 'Save Time' },
  { key: 'supportLocal', icon: '🏪', label: 'Support Local' },
  { key: 'bestOverall', icon: '⚖️', label: 'Best Overall' },
]

function setBudget(e: Event) {
  const val = parseFloat((e.target as HTMLInputElement).value)
  listStore.budget = isNaN(val) ? undefined : val
}
</script>

<template>
  <div class="page">
    <div class="list-header">
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">My List 🛒</h1>
    </div>

    <!-- Budget -->
    <div class="card mb-12">
      <div class="section-title" style="margin-bottom:10px">Budget</div>
      <div class="budget-input-row">
        <span class="currency">R</span>
        <input
          type="number"
          placeholder="e.g. 500"
          :value="listStore.budget ?? ''"
          @input="setBudget"
          style="padding-left: 32px"
        />
      </div>
    </div>

    <!-- Priority -->
    <div class="card mb-12">
      <div class="section-title" style="margin-bottom:10px">Priority</div>
      <div class="priority-row">
        <button
          v-for="p in priorities" :key="p.key"
          class="chip" :class="{ active: listStore.priority === p.key }"
          @click="listStore.priority = p.key"
        >{{ p.icon }} {{ p.label }}</button>
      </div>
    </div>

    <!-- Items -->
    <div class="card mb-12">
      <div class="row-between mb-10">
        <div class="section-title" style="margin:0">Items ({{ listStore.items.length }})</div>
        <button v-if="listStore.items.length" class="btn btn-sm btn-outline" @click="router.push('/search')">+ Add</button>
      </div>

      <div v-if="listStore.items.length === 0" class="empty-list">
        <div>No items yet.</div>
        <button class="btn btn-primary mt-12" @click="router.push('/search')">Search Products</button>
      </div>

      <div v-else class="items-list">
        <div v-for="item in listStore.items" :key="item.productId" class="list-item">
          <button class="check-btn" @click="listStore.togglePurchased(item.productId)">
            {{ item.isPurchased ? '✅' : '⬜' }}
          </button>
          <div class="item-name" :class="{ purchased: item.isPurchased }">{{ item.productName }}</div>
          <div class="qty-controls">
            <button class="qty-btn" @click="listStore.updateQuantity(item.productId, item.quantity - 1)">−</button>
            <span class="qty-val">{{ item.quantity }}</span>
            <button class="qty-btn" @click="listStore.updateQuantity(item.productId, item.quantity + 1)">+</button>
          </div>
          <button class="remove-btn" @click="listStore.removeItem(item.productId)">🗑</button>
        </div>
      </div>
    </div>

    <!-- Optimise CTA -->
    <button
      v-if="listStore.items.length > 0"
      class="btn btn-primary btn-full"
      @click="router.push('/optimise')"
    >
      ⚡ Optimise My Shopping
    </button>
  </div>
</template>

<style scoped>
.list-header { margin-bottom: 16px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; }
.mb-10 { margin-bottom: 10px; }
.mb-12 { margin-bottom: 12px; }
.mt-12 { margin-top: 12px; }
.row-between { display: flex; justify-content: space-between; align-items: center; }
.budget-input-row { position: relative; }
.currency { position: absolute; left: 14px; top: 50%; transform: translateY(-50%); font-weight: 700; font-size: 16px; z-index: 1; }
.priority-row { display: flex; flex-wrap: wrap; gap: 8px; }
.items-list { display: flex; flex-direction: column; gap: 2px; }
.list-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 0;
  border-bottom: 1px solid var(--mm-border);
}
.list-item:last-child { border-bottom: none; }
.check-btn { background: none; border: none; cursor: pointer; font-size: 20px; flex-shrink: 0; }
.item-name { flex: 1; font-size: 15px; font-weight: 500; }
.item-name.purchased { text-decoration: line-through; color: var(--mm-text-secondary); }
.qty-controls { display: flex; align-items: center; gap: 8px; }
.qty-btn {
  width: 28px; height: 28px;
  border-radius: 50%;
  border: 2px solid var(--mm-border);
  background: var(--mm-surface);
  font-size: 16px;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  font-weight: 700;
}
.qty-val { font-size: 15px; font-weight: 600; min-width: 20px; text-align: center; }
.remove-btn { background: none; border: none; cursor: pointer; font-size: 18px; opacity: 0.6; }
.empty-list { text-align: center; padding: 20px 0; color: var(--mm-text-secondary); }
</style>
