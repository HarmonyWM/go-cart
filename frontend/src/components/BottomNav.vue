<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useListStore } from '../stores/listStore'
import { computed } from 'vue'

const route = useRoute()
const listStore = useListStore()
const count = computed(() => listStore.totalItems)

const tabs = [
  { to: '/', icon: '🏠', label: 'Home' },
  { to: '/search', icon: '🔍', label: 'Search' },
  { to: '/deals', icon: '🔥', label: 'Deals' },
  { to: '/list', icon: '🛒', label: 'List' },
  { to: '/profile', icon: '👤', label: 'Profile' },
]
</script>

<template>
  <nav class="bottom-nav">
    <RouterLink
      v-for="tab in tabs"
      :key="tab.to"
      :to="tab.to"
      class="nav-item"
      :class="{ active: route.path === tab.to }"
    >
      <span class="nav-icon">
        {{ tab.icon }}
        <span v-if="tab.to === '/list' && count > 0" class="nav-badge">{{ count }}</span>
      </span>
      <span class="nav-label">{{ tab.label }}</span>
    </RouterLink>
  </nav>
</template>

<style scoped>
.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  width: 100%;
  height: var(--nav-height);
  background: var(--mm-card);
  border-top: 1px solid var(--mm-border);
  display: flex;
  z-index: 100;
  box-shadow: 0 -2px 12px rgba(0,0,0,0.06);
}
.nav-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 3px;
  text-decoration: none;
  color: var(--mm-text-secondary);
  transition: color 0.15s;
}
.nav-item.active { color: var(--mm-green-dark); }
.nav-icon { font-size: 22px; position: relative; line-height: 1; }
.nav-label { font-size: 11px; font-weight: 500; }
.nav-badge {
  position: absolute;
  top: -4px; right: -8px;
  background: var(--mm-green);
  color: #fff;
  font-size: 10px;
  font-weight: 700;
  min-width: 16px;
  height: 16px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 3px;
}
</style>
