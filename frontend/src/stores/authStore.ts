import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export type AuthUser = {
  id: string
  name: string
  email: string
  avatar: string
  provider: 'google' | 'microsoft' | 'demo'
}

// Mock SSO provider configs — replace clientId with real values when integrating
const SSO_PROVIDERS = {
  google: {
    name: 'Google',
    icon: '🔵',
    color: '#4285F4',
    // Real integration: use Google Identity Services SDK
    // https://developers.google.com/identity/gsi/web
  },
  microsoft: {
    name: 'Microsoft',
    icon: '🟦',
    color: '#00A4EF',
    // Real integration: use MSAL.js
    // https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-overview
  },
}

const STORAGE_KEY = 'malimove_auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const loading = ref(false)

  const isAuthenticated = computed(() => user.value !== null)

  // Restore session from localStorage on app load
  function restore() {
    try {
      const stored = localStorage.getItem(STORAGE_KEY)
      if (stored) user.value = JSON.parse(stored)
    } catch {}
  }

  function persist() {
    if (user.value) localStorage.setItem(STORAGE_KEY, JSON.stringify(user.value))
    else localStorage.removeItem(STORAGE_KEY)
  }

  // DEMO SSO — simulates the OAuth redirect/popup flow
  // Replace the body of each case with real SDK calls when integrating
  async function loginWithProvider(provider: 'google' | 'microsoft') {
    loading.value = true
    try {
      // ⚠️ DEMO: Simulates a successful SSO response
      // Real Google: await google.accounts.id.initialize({ client_id: '...' })
      // Real Microsoft: await msalInstance.loginPopup({ scopes: ['User.Read'] })
      await new Promise(r => setTimeout(r, 1200)) // simulate network

      const mockProfiles: Record<string, AuthUser> = {
        google: {
          id: 'google-demo-001',
          name: 'Demo User',
          email: 'demo@gmail.com',
          avatar: 'DU',
          provider: 'google',
        },
        microsoft: {
          id: 'ms-demo-001',
          name: 'Demo User',
          email: 'demo@outlook.com',
          avatar: 'DU',
          provider: 'microsoft',
        },
      }

      user.value = mockProfiles[provider] ?? null
      persist()
    } finally {
      loading.value = false
    }
  }

  // Quick demo login — no provider needed
  async function loginDemo(name: string, email: string) {
    loading.value = true
    await new Promise(r => setTimeout(r, 600))
    user.value = {
      id: `demo-${Date.now()}`,
      name: name || 'Shopper',
      email: email || 'shopper@malimove.co.za',
      avatar: (name || 'S').charAt(0).toUpperCase(),
      provider: 'demo',
    }
    persist()
    loading.value = false
  }

  function logout() {
    user.value = null
    persist()
  }

  return { user, loading, isAuthenticated, restore, loginWithProvider, loginDemo, logout, SSO_PROVIDERS }
})
