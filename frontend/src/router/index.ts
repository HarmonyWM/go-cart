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
    { path: '/checkout/method', name: 'checkout-method', component: () => import('../views/checkout/MethodView.vue') },
    { path: '/checkout/delivery', name: 'checkout-delivery', component: () => import('../views/checkout/DeliveryView.vue') },
    { path: '/checkout/review', name: 'checkout-review', component: () => import('../views/checkout/ReviewView.vue') },
    { path: '/checkout/payment', name: 'checkout-payment', component: () => import('../views/checkout/PaymentView.vue') },
    { path: '/checkout/confirmation', name: 'checkout-confirmation', component: () => import('../views/checkout/ConfirmationView.vue') },
    { path: '/orders/:orderNumber', name: 'order-tracking', component: () => import('../views/checkout/OrderTrackingView.vue') },
  ],
})

export default router
