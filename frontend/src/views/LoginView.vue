<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/authStore'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

const name = ref('')
const email = ref('')
const showDemo = ref(false)
const error = ref('')

async function sso(provider: 'google' | 'microsoft') {
  error.value = ''
  await auth.loginWithProvider(provider)
  router.push('/')
}

async function demoLogin() {
  if (!name.value.trim()) { error.value = 'Please enter your name.'; return }
  error.value = ''
  await auth.loginDemo(name.value.trim(), email.value.trim())
  router.push('/')
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <!-- Brand -->
      <div class="auth-brand">
        <div class="auth-logo">🛒</div>
        <div class="auth-name">MaliMove</div>
        <div class="auth-tagline">Your shopping mission, optimised.</div>
      </div>

      <div class="auth-divider" />

      <!-- SSO Buttons -->
      <div class="sso-section">
        <button class="sso-btn google" :disabled="auth.loading" @click="sso('google')">
          <span class="sso-icon">
            <svg width="18" height="18" viewBox="0 0 24 24"><path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/><path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z"/><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/></svg>
          </span>
          <span>Continue with Google</span>
          <span v-if="auth.loading" class="spinner-sm" />
        </button>

        <button class="sso-btn microsoft" :disabled="auth.loading" @click="sso('microsoft')">
          <span class="sso-icon">
            <svg width="18" height="18" viewBox="0 0 24 24"><path fill="#F25022" d="M1 1h10v10H1z"/><path fill="#7FBA00" d="M13 1h10v10H13z"/><path fill="#00A4EF" d="M1 13h10v10H1z"/><path fill="#FFB900" d="M13 13h10v10H13z"/></svg>
          </span>
          <span>Continue with Microsoft</span>
          <span v-if="auth.loading" class="spinner-sm" />
        </button>
      </div>

      <div class="or-divider"><span>or</span></div>

      <!-- Demo login -->
      <div v-if="!showDemo">
        <button class="btn btn-ghost btn-full demo-toggle" @click="showDemo = true">
          Continue as Guest (Demo)
        </button>
      </div>

      <div v-else class="demo-form">
        <input v-model="name" placeholder="Your name" class="mb-10" />
        <input v-model="email" placeholder="Email (optional)" type="email" class="mb-10" />
        <div v-if="error" class="auth-error">{{ error }}</div>
        <button class="btn btn-primary btn-full" :disabled="auth.loading" @click="demoLogin">
          <span v-if="auth.loading"><span class="spinner-sm" /> Signing in…</span>
          <span v-else>Start Shopping →</span>
        </button>
        <button class="btn btn-ghost btn-full mt-8" @click="showDemo = false">← Back</button>
      </div>

      <div class="auth-notice">
        ⚠️ SSO is simulated in this prototype. No real OAuth flow is active.
      </div>
    </div>
  </div>
</template>

<style scoped>
.auth-card {
  background: #fff;
  border-radius: 20px;
  padding: 40px 36px;
  width: 100%;
  max-width: 420px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.25);
}
.auth-brand { text-align: center; margin-bottom: 28px; }
.auth-logo { font-size: 52px; margin-bottom: 10px; }
.auth-name { font-size: 30px; font-weight: 800; color: var(--mm-green-dark); letter-spacing: -0.5px; }
.auth-tagline { font-size: 14px; color: var(--mm-text-secondary); margin-top: 4px; }
.auth-divider { height: 1px; background: var(--mm-border); margin-bottom: 24px; }
.sso-section { display: flex; flex-direction: column; gap: 12px; margin-bottom: 16px; }
.sso-btn {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 13px 18px;
  border-radius: 10px;
  border: 2px solid var(--mm-border);
  background: #fff;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
  width: 100%;
}
.sso-btn:hover:not(:disabled) { border-color: #aaa; background: #fafafa; }
.sso-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.sso-icon { display: flex; align-items: center; flex-shrink: 0; }
.sso-btn span:nth-child(2) { flex: 1; text-align: left; }
.or-divider {
  text-align: center;
  position: relative;
  margin: 16px 0;
  color: var(--mm-text-secondary);
  font-size: 13px;
}
.or-divider::before, .or-divider::after {
  content: '';
  position: absolute;
  top: 50%;
  width: 42%;
  height: 1px;
  background: var(--mm-border);
}
.or-divider::before { left: 0; }
.or-divider::after { right: 0; }
.demo-toggle { color: var(--mm-text-secondary); font-size: 14px; }
.demo-form { display: flex; flex-direction: column; }
.mb-10 { margin-bottom: 10px; }
.mt-8 { margin-top: 8px; }
.auth-error { color: var(--mm-red); font-size: 13px; margin-bottom: 8px; }
.auth-notice { text-align: center; font-size: 11px; color: var(--mm-text-secondary); margin-top: 20px; line-height: 1.5; }
.spinner-sm {
  display: inline-block; width: 14px; height: 14px;
  border: 2px solid rgba(0,0,0,0.15); border-top-color: currentColor;
  border-radius: 50%; animation: spin 0.7s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>
