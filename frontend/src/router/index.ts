import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'home', component: HomeView },
    { path: '/search', name: 'search', component: () => import('../views/SearchView.vue') },
    { path: '/deals', name: 'deals', component: () => import('../views/DealsView.vue') },
    { path: '/list', name: 'list', component: () => import('../views/ListV.vue') },
    { path: '/optimise', name: 'optimise', component: () => import('../views/OptimiseView.vue') },
    { path: '/profile', name: 'profile', component: () => import('../views/ProfileView.vue') },
  ],
})

export default router
