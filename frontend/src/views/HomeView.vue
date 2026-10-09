<script setup lang="ts">
import { useListStore } from '../stores/listStore'
import { useRouter } from 'vue-router'
import { computed } from 'vue'

const listStore = useListStore()
const router = useRouter()

const priorities = [
  { key: 'saveMoney', icon: '💰', label: 'Save Money' },
  { key: 'saveTravel', icon: '🚗', label: 'Save Travel' },
  { key: 'saveTime', icon: '⏱️', label: 'Save Time' },
  { key: 'supportLocal', icon: '🏪', label: 'Support Local' },
  { key: 'bestOverall', icon: '⚖️', label: 'Best Overall' },
]

const hasItems = computed(() => listStore.items.length > 0)
</script>

<template>
  <div class="page">
    <!-- Header -->
    <div class="home-header">
      <div>
        <div class="brand">MaliMove</div>
        <div class="tagline">Your shopping mission, optimised.</div>
      </div>
      <div class="header-icon">🛒</div>
    </div>

    <!-- Budget bar -->
    <div class="card mb-12" v-if="hasItems">
      <div class="row-between mb-8">
        <span class="section-title" style="margin:0">Current Mission</span>
        <span class="badge badge-green">{{ listStore.items.length }} items</span>
      </div>
      <div class="row-between text-secondary mb-8">
        <span>Budget</span>
        <span class="fw-600">{{ listStore.budget ? `R${listStore.budget}` : 'Not set' }}</span>
      </div>
      <div class="row-between text-secondary">
        <span>Priority</span>
        <span class="fw-600">{{ priorities.find(p => p.key === listStore.priority)?.icon }} {{ priorities.find(p => p.key === listStore.priority)?.label }}</span>
      </div>
      <div class="divider" />
      <button class="btn btn-primary btn-full" @click="router.push('/optimise')">
        ⚡ Optimise My Shopping
      </button>
    </div>

    <!-- Empty state -->
    <div class="card mb-12" v-else>
      <div class="empty-home">
        <div class="empty-emoji">🛒</div>
        <div class="empty-title">Start your shopping mission</div>
        <div class="text-secondary mb-16">Add items to your list, set a budget and let MaliMove find the smartest way to shop.</div>
        <button class="btn btn-primary" @click="router.push('/search')">+ Add Items</button>
      </div>
    </div>

    <!-- Priority picker -->
    <div class="section-title">What matters most today?</div>
    <div class="priority-grid mb-16">
      <button
        v-for="p in priorities"
        :key="p.key"
        class="priority-card"
        :class="{ active: listStore.priority === p.key }"
        @click="listStore.priority = p.key"
      >
        <span class="priority-icon">{{ p.icon }}</span>
        <span class="priority-label">{{ p.label }}</span>
      </button>
    </div>

    <!-- Quick links -->
    <div class="section-title">Quick Actions</div>
    <div class="quick-grid">
      <button class="quick-card" @click="router.push('/search')">
        <span>🔍</span><span>Search Products</span>
      </button>
      <button class="quick-card" @click="router.push('/deals')">
        <span>🔥</span><span>Browse Deals</span>
      </button>
      <button class="quick-card" @click="router.push('/list')">
        <span>📋</span><span>My List</span>
      </button>
      <button class="quick-card" @click="router.push('/profile')">
        <span>⚙️</span><span>Preferences</span>
      </button>
    </div>

    <!-- Tagline -->
    <div class="tagline-footer">
      When every rand matters, why should you have to do the maths?
    </div>
  </div>
</template>

<style scoped>
.home-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding: 8px 0;
}
.brand { font-size: 26px; font-weight: 800; color: var(--mm-green-dark); letter-spacing: -0.5px; }
.tagline { font-size: 13px; color: var(--mm-text-secondary); }
.header-icon { font-size: 36px; }
.mb-8 { margin-bottom: 8px; }
.mb-12 { margin-bottom: 12px; }
.mb-16 { margin-bottom: 16px; }
.row-between { display: flex; justify-content: space-between; align-items: center; }
.fw-600 { font-weight: 600; }
.empty-home { text-align: center; padding: 12px 0; }
.empty-emoji { font-size: 48px; margin-bottom: 12px; }
.empty-title { font-size: 18px; font-weight: 700; margin-bottom: 8px; }
.priority-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
  margin-bottom: 20px;
}
.priority-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 14px 8px;
  border-radius: var(--mm-radius);
  border: 2px solid var(--mm-border);
  background: var(--mm-card);
  cursor: pointer;
  transition: all 0.15s;
}
.priority-card.active {
  border-color: var(--mm-green);
  background: var(--mm-green-light);
}
.priority-icon { font-size: 24px; }
.priority-label { font-size: 12px; font-weight: 600; color: var(--mm-text); text-align: center; }
.quick-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-bottom: 24px;
}
.quick-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 16px;
  background: var(--mm-card);
  border-radius: var(--mm-radius);
  border: none;
  box-shadow: var(--mm-shadow);
  cursor: pointer;
  font-size: 13px;
  font-weight: 600;
  color: var(--mm-text);
  transition: transform 0.1s;
}
.quick-card:active { transform: scale(0.97); }
.quick-card span:first-child { font-size: 28px; }
.tagline-footer {
  text-align: center;
  font-size: 13px;
  color: var(--mm-text-secondary);
  font-style: italic;
  padding: 8px 0 4px;
}
</style>
