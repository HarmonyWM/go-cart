<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { usePrefsStore } from '../stores/prefsStore'

const prefsStore = usePrefsStore()
const saved = ref(false)

onMounted(() => prefsStore.load())

async function save() {
  await prefsStore.save()
  saved.value = true
  setTimeout(() => { saved.value = false }, 2000)
}

const transportOptions = [
  { value: 'driving', label: '🚗 Driving' },
  { value: 'walking', label: '🚶 Walking' },
  { value: 'publicTransport', label: '🚌 Public Transport' },
]

const fuelOptions = [
  { value: 'petrol95', label: 'Petrol 95' },
  { value: 'petrol93', label: 'Petrol 93' },
  { value: 'diesel', label: 'Diesel' },
]
</script>

<template>
  <div class="page">
    <div class="profile-header">
      <div class="brand-sm">MaliMove</div>
      <h1 class="page-title">Profile ⚙️</h1>
    </div>

    <div class="card mb-12">
      <div class="section-title" style="margin-bottom:14px">Travel Settings</div>

      <div class="field">
        <label>Transport Mode</label>
        <div class="transport-row">
          <button
            v-for="t in transportOptions" :key="t.value"
            class="chip" :class="{ active: prefsStore.prefs.transportMode === t.value }"
            @click="prefsStore.prefs.transportMode = t.value"
          >{{ t.label }}</button>
        </div>
      </div>

      <div class="field" v-if="prefsStore.prefs.transportMode === 'driving'">
        <label>Fuel Type</label>
        <select v-model="prefsStore.prefs.fuelType">
          <option v-for="f in fuelOptions" :key="f.value" :value="f.value">{{ f.label }}</option>
        </select>
      </div>

      <div class="field" v-if="prefsStore.prefs.transportMode === 'driving'">
        <label>Fuel Price (R/litre)</label>
        <input type="number" step="0.01" v-model.number="prefsStore.prefs.fuelPricePerLitre" />
      </div>

      <div class="field" v-if="prefsStore.prefs.transportMode === 'driving'">
        <label>Vehicle Consumption (L/100km)</label>
        <input type="number" step="0.1" v-model.number="prefsStore.prefs.vehicleConsumptionLPer100km" />
      </div>

      <div class="field">
        <label>Max Travel Distance (km)</label>
        <input type="number" v-model.number="prefsStore.prefs.maxTravelDistanceKm" />
      </div>
    </div>

    <div class="card mb-12">
      <div class="section-title" style="margin-bottom:14px">Shopping Preferences</div>

      <div class="field">
        <label>Default Priority</label>
        <select v-model="prefsStore.prefs.defaultPriority">
          <option value="saveMoney">💰 Save Money</option>
          <option value="saveTravel">🚗 Save Travel</option>
          <option value="saveTime">⏱️ Save Time</option>
          <option value="supportLocal">🏪 Support Local</option>
          <option value="bestOverall">⚖️ Best Overall</option>
        </select>
      </div>

      <div class="field toggle-field">
        <label>Willing to switch brands to save money?</label>
        <button
          class="toggle-btn"
          :class="{ on: prefsStore.prefs.willingToSwitchBrands }"
          @click="prefsStore.prefs.willingToSwitchBrands = !prefsStore.prefs.willingToSwitchBrands"
        >
          {{ prefsStore.prefs.willingToSwitchBrands ? 'Yes' : 'No' }}
        </button>
      </div>
    </div>

    <button class="btn btn-primary btn-full" @click="save">
      {{ saved ? '✓ Saved!' : 'Save Preferences' }}
    </button>

    <div class="app-info">
      <div class="app-name">MaliMove</div>
      <div class="app-tagline">Your shopping mission, optimised.</div>
      <div class="app-sub">Compare. Switch. Combine. Save.</div>
      <div class="mock-notice">⚠️ Prices are estimated mock data for demonstration purposes.</div>
    </div>
  </div>
</template>

<style scoped>
.profile-header { margin-bottom: 16px; }
.brand-sm { font-size: 13px; font-weight: 700; color: var(--mm-green-dark); margin-bottom: 2px; }
.page-title { font-size: 22px; font-weight: 800; }
.mb-12 { margin-bottom: 12px; }
.field { margin-bottom: 14px; }
.field label { display: block; font-size: 13px; font-weight: 600; color: var(--mm-text-secondary); margin-bottom: 6px; text-transform: uppercase; letter-spacing: 0.3px; }
.transport-row { display: flex; flex-wrap: wrap; gap: 8px; }
.toggle-field { display: flex; justify-content: space-between; align-items: center; }
.toggle-field label { margin: 0; text-transform: none; font-size: 14px; color: var(--mm-text); }
.toggle-btn {
  padding: 8px 18px;
  border-radius: 20px;
  border: 2px solid var(--mm-border);
  background: var(--mm-surface);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
  color: var(--mm-text-secondary);
}
.toggle-btn.on { background: var(--mm-green-light); border-color: var(--mm-green); color: var(--mm-green-dark); }
.app-info { text-align: center; padding: 24px 0 8px; }
.app-name { font-size: 22px; font-weight: 800; color: var(--mm-green-dark); }
.app-tagline { font-size: 14px; font-weight: 600; margin: 4px 0; }
.app-sub { font-size: 13px; color: var(--mm-text-secondary); margin-bottom: 12px; }
.mock-notice { font-size: 12px; color: var(--mm-orange); background: #FFF3E0; padding: 8px 12px; border-radius: var(--mm-radius-sm); }
</style>
