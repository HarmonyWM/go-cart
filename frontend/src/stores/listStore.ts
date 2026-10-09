import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ShoppingListItemDto } from '../services/api'

export const useListStore = defineStore('list', () => {
  const items = ref<ShoppingListItemDto[]>([])
  const budget = ref<number | undefined>(undefined)
  const priority = ref('bestOverall')
  const listName = ref('My Shopping List')

  const totalItems = computed(() => items.value.reduce((s, i) => s + i.quantity, 0))

  function addItem(productId: string, productName: string) {
    const existing = items.value.find(i => i.productId === productId)
    if (existing) { existing.quantity++; return }
    items.value.push({ productId, productName, quantity: 1, isPurchased: false })
  }

  function removeItem(productId: string) {
    items.value = items.value.filter(i => i.productId !== productId)
  }

  function updateQuantity(productId: string, qty: number) {
    const item = items.value.find(i => i.productId === productId)
    if (item) item.quantity = Math.max(1, qty)
  }

  function togglePurchased(productId: string) {
    const item = items.value.find(i => i.productId === productId)
    if (item) item.isPurchased = !item.isPurchased
  }

  function clearList() { items.value = [] }

  return { items, budget, priority, listName, totalItems, addItem, removeItem, updateQuantity, togglePurchased, clearList }
})
