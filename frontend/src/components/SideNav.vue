<script setup lang="ts">
import { useRoute } from 'vue-router'

const route = useRoute()

const nav = [
  { to: '/', icon: '🏠', label: 'Home' },
  { to: '/search', icon: '🔍', label: 'Search' },
  { to: '/deals', icon: '🔥', label: 'Deals' },
  { to: '/list', icon: '🛒', label: 'My List' },
  { to: '/profile', icon: '⚙️', label: 'Profile' },
]
</script>

<template>
  <aside class="side-nav">
    <nav>
      <RouterLink
        v-for="item in nav"
        :key="item.to"
        :to="item.to"
        class="side-item"
        :class="{ active: route.path === item.to }"
      >
        <span class="side-icon">{{ item.icon }}</span>
        <span class="side-label">{{ item.label }}</span>
      </RouterLink>
    </nav>
  </aside>
</template>

<style scoped>
.side-nav {
  width: var(--sidebar-width);
  flex-shrink: 0;
  background: var(--mm-card);
  border-right: 1px solid var(--mm-border);
  padding: 16px 12px;
  position: sticky;
  top: var(--header-height);
  height: calc(100vh - var(--header-height));
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}
nav { display: flex; flex-direction: column; gap: 4px; }
.side-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 14px;
  border-radius: var(--mm-radius-sm);
  text-decoration: none;
  color: var(--mm-text-secondary);
  font-size: 15px;
  font-weight: 500;
  transition: all 0.15s;
}
.side-item:hover { background: var(--mm-surface); color: var(--mm-text); }
.side-item.active { background: var(--mm-green-light); color: var(--mm-green-dark); font-weight: 700; }
.side-icon { font-size: 20px; flex-shrink: 0; }
</style>
