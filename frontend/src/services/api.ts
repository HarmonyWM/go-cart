const BASE = 'http://localhost:5000/api'

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE}${path}`)
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}

async function post<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}

async function put<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}

export type StorePriceDto = {
  storeId: string; storeName: string; retailerName: string
  price: number; hasDeal: boolean; dealPrice?: number
}

export type ProductSearchResult = {
  id: string; name: string; category: string; brand: string; unit: string
  lowestPrice?: number; lowestPriceStoreId?: string; lowestPriceStoreName?: string
  storePrices: StorePriceDto[]; alternatives: string[]
}

export type DealDto = {
  id: string; productId: string; productName: string
  storeId: string; storeName: string; retailerId: string; retailerName: string
  title: string; description: string; dealPrice: number; originalPrice: number
  discountPercent: number; savingsAmount: number; validUntil: string
  category: string; isActive: boolean
}

export type StoreDto = {
  id: string; retailerId: string; retailerName: string; name: string
  address: string; distanceKm: number; estimatedShoppingMinutes: number
  isLocal: boolean; color: string
}

export type ShoppingListItemDto = {
  productId: string; productName: string; quantity: number; isPurchased: boolean
}

export type UserPreferencesDto = {
  preferredStoreIds: string[]; maxTravelDistanceKm: number; transportMode: string
  fuelType: string; vehicleConsumptionLPer100km: number; fuelPricePerLitre: number
  willingToSwitchBrands: boolean; defaultPriority: string
}

export type BasketItemDto = {
  productId: string; productName: string; brand: string; quantity: number
  unitPrice: number; lineTotal: number; hasDeal: boolean; originalPrice?: number
}

export type StoreBasketDto = {
  storeId: string; storeName: string; retailerName: string; color: string
  items: BasketItemDto[]; subTotal: number; distanceKm: number; travelCost: number
}

export type ShoppingOptionDto = {
  key: string; label: string; icon: string; storeBaskets: StoreBasketDto[]
  productTotal: number; travelCost: number; deliveryCost: number; totalEstimatedCost: number
  storeCount: number; estimatedSavings: number; estimatedTotalMinutes: number
  totalDistanceKm: number; description: string; isRecommended: boolean
}

export type OptimisationResult = {
  options: ShoppingOptionDto[]; recommended: ShoppingOptionDto
  recommendationReason: string; budget?: number; budgetStatus: string
}

export type OrderItemDto = {
  productId: string; productName: string; brand: string; quantity: number
  unitPrice: number; lineTotal: number; hasDeal: boolean; originalPrice?: number
  isSubstituted: boolean; substitutionNote?: string
}

export type OrderStoreBasketDto = {
  storeId: string; storeName: string; retailerName: string; color: string
  items: OrderItemDto[]; subTotal: number; distanceKm: number; travelCost: number
}

export type DeliveryDetailsDto = {
  address: string; contactNumber: string; deliveryWindow: string; deliveryNotes?: string
}

export type OrderStatusEventDto = { status: string; message: string; timestamp: string }

export type OrderDto = {
  id: string; orderNumber: string; method: string
  baskets: OrderStoreBasketDto[]
  productTotal: number; travelCost: number; deliveryFee: number
  serviceFee: number; grandTotal: number
  delivery?: DeliveryDetailsDto
  status: string; paymentStatus: string
  createdAt: string; statusHistory: OrderStatusEventDto[]
}

export type PlaceOrderRequest = {
  method: string
  baskets: OrderStoreBasketDto[]
  productTotal: number; travelCost: number; deliveryFee: number
  serviceFee: number; grandTotal: number
  delivery?: DeliveryDetailsDto
}

export const api = {
  searchProducts: (q: string) =>
    get<ProductSearchResult[]>(`/products/search?q=${encodeURIComponent(q)}`),
  getProduct: (id: string) => get<ProductSearchResult>(`/products/${id}`),
  getAlternatives: (id: string) => get<unknown[]>(`/products/${id}/alternatives`),
  getStores: () => get<StoreDto[]>('/stores'),
  getDeals: (params?: { storeId?: string; category?: string; search?: string }) => {
    const q = new URLSearchParams()
    if (params?.storeId) q.set('storeId', params.storeId)
    if (params?.category) q.set('category', params.category)
    if (params?.search) q.set('search', params.search)
    return get<DealDto[]>(`/deals?${q}`)
  },
  getDealsMatchingList: (productIds: string[]) =>
    post<DealDto[]>('/deals/matching-list', productIds),
  optimise: (
    items: ShoppingListItemDto[],
    budget: number | undefined,
    priority: string,
    prefs: UserPreferencesDto
  ) => post<OptimisationResult>('/optimisation/calculate', { items, budget, priority, preferences: prefs }),
  getPreferences: () => get<UserPreferencesDto>('/user/preferences'),
  savePreferences: (prefs: UserPreferencesDto) =>
    put<UserPreferencesDto>('/user/preferences', prefs),
  placeOrder: (req: PlaceOrderRequest) => post<OrderDto>('/orders', req),
  getOrder: (orderNumber: string) => get<OrderDto>(`/orders/${orderNumber}`),
  demoPayment: (orderId: string, method: string) =>
    post<{ success: boolean; reference: string; message: string }>('/orders/payment/demo', { orderId, method }),
  updateOrderStatus: (orderId: string, status: string, message: string) =>
    post<OrderDto>(`/orders/${orderId}/status`, { status, message }),
}
