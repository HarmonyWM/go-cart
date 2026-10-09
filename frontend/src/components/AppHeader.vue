<script setup lang="ts">
import { useAuthStore } from '../stores/authStore'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <header class="app-header">
    <div class="header-brand" @click="router.push('/')">
      <span class="header-logo">🛒</span>
      <span class="header-name">MaliMove</span>
    </div>
    <div class="header-right">
      <div class="user-pill">
        <div class="user-avatar">{{ auth.user?.avatar }}</div>
        <span class="user-name">{{ auth.user?.name }}</span>
        <button class="logout-btn" @click="logout" title="Sign out">⏻</button>
      </div>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  height: var(--header-height);
  background: var(--mm-card);
  border-bottom: 1px solid var(--mm-border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  position: sticky;
  top: 0;
  z-index: 50;
  box-shadow: 0 1px 4px rgba(0,0,0,0.06);
}
.header-brand {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  text-decoration: none;
}
.header-logo { font-size: 24px; }
.header-name { font-size: 20px; font-weight: 800; color: var(--mm-green-dark); letter-spacing: -0.3px; }
.header-right { display: flex; align-items: center; gap: 12px; }
.user-pill {
  display: flex;
  align-items: center;
  gap: 8px;
  background: var(--mm-surface);
  border-radius: 24px;
  padding: 5px 12px 5px 5px;
}
.user-avatar {
  width: 30px; height: 30px;
  border-radius: 50%;
  background: var(--mm-green);
  color: #fff;
  font-size: 13px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.user-name { font-size: 14px; font-weight: 600; max-width: 120px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.logout-btn {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 16px;
  color: var(--mm-text-secondary);
  padding: 2px;
  line-height: 1;
}
.logout-btn:hover { color: var(--mm-red); }
</style>
