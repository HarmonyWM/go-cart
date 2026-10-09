import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { UserPreferencesDto } from '../services/api'
import { api } from '../services/api'

export const usePrefsStore = defineStore('prefs', () => {
  const prefs = ref<UserPreferencesDto>({
    preferredStoreIds: [],
    maxTravelDistanceKm: 15,
    transportMode: 'driving',
    fuelType: 'petrol95',
    vehicleConsumptionLPer100km: 7.2,
    fuelPricePerLitre: 24.50,
    willingToSwitchBrands: true,
    defaultPriority: 'bestOverall',
  })

  async function load() {
    try { prefs.value = await api.getPreferences() } catch {}
  }

  async function save() {
    try { prefs.value = await api.savePreferences(prefs.value) } catch {}
  }

  return { prefs, load, save }
})
