<script setup lang="ts">
import { RouterView } from 'vue-router'
import BottomNav from './components/BottomNav.vue'
</script>

<template>
  <div id="app-shell">
    <RouterView />
    <BottomNav />
  </div>
</template>

<style scoped>
#app-shell { position: relative; }
</style>
