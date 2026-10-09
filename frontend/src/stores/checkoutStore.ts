import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ShoppingOptionDto, OrderDto, DeliveryDetailsDto } from '../services/api'

export const useCheckoutStore = defineStore('checkout', () => {
  const selectedOption = ref<ShoppingOptionDto | null>(null)
  const method = ref<'ShopMyself' | 'PersonalShopper'>('ShopMyself')
  const delivery = ref<DeliveryDetailsDto>({
    address: '', contactNumber: '', deliveryWindow: '', deliveryNotes: ''
  })
  const placedOrder = ref<OrderDto | null>(null)
  const paymentReference = ref('')

  const DELIVERY_FEE = 45.00
  const SERVICE_FEE = 25.00

  function grandTotal(opt: ShoppingOptionDto) {
    if (method.value === 'PersonalShopper')
      return opt.productTotal + opt.travelCost + DELIVERY_FEE + SERVICE_FEE
    return opt.productTotal + opt.travelCost
  }

  function reset() {
    selectedOption.value = null
    method.value = 'ShopMyself'
    delivery.value = { address: '', contactNumber: '', deliveryWindow: '', deliveryNotes: '' }
    placedOrder.value = null
    paymentReference.value = ''
  }

  return { selectedOption, method, delivery, placedOrder, paymentReference, DELIVERY_FEE, SERVICE_FEE, grandTotal, reset }
})
