<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterView, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from './stores/authStore'
import BottomNav from './components/BottomNav.vue'
import SideNav from './components/SideNav.vue'
import AppHeader from './components/AppHeader.vue'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const isAuthRoute = computed(() => route.name === 'login')

onMounted(() => {
  auth.restore()
  if (!auth.isAuthenticated && route.name !== 'login') {
    router.push('/login')
  }
})
</script>

<template>
  <!-- Auth pages: full screen, no chrome -->
  <RouterView v-if="isAuthRoute" />

  <!-- App shell: full page with header + nav -->
  <div v-else-if="auth.isAuthenticated" class="app-shell">
    <AppHeader />
    <div class="app-body">
      <SideNav class="side-nav-desktop" />
      <main class="app-main">
        <RouterView />
      </main>
    </div>
    <BottomNav class="bottom-nav-mobile" />
  </div>

  <!-- Loading state while restoring session -->
  <div v-else class="auth-page">
    <div class="loading" style="color:#fff"><div class="spinner" /> Loading…</div>
  </div>
</template>

<style>
.side-nav-desktop { display: none; }
.bottom-nav-mobile { display: flex; }

@media (min-width: 768px) {
  .side-nav-desktop { display: flex; }
  .bottom-nav-mobile { display: none; }
}

.app-shell { display: flex; flex-direction: column; min-height: 100vh; }
.app-body { display: flex; flex: 1; min-height: 0; }
.app-main { flex: 1; overflow-y: auto; min-width: 0; }
</style>
