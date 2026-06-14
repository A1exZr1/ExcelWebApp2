<template>
  <v-app style="height: 100vh; width: 100vw;">
    <v-main style="height: 100%; width: 100%;">
      <div v-if="loading" class="app-loading">
        <v-progress-circular indeterminate color="primary" />
      </div>

      <LoginPage
        v-else-if="!isAuthenticated"
        @authenticated="onAuthenticated"
      />

      <div v-else class="app-shell">
        <div class="app-header">
          <div class="user-email">{{ userEmail }}</div>
          <v-btn variant="text" color="primary" :loading="logoutLoading" @click="logout">
            Выйти
          </v-btn>
        </div>
        <div class="app-content">
          <ResultPage />
        </div>
      </div>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { authClient } from './api/fileReaderApi'
import LoginPage from './views/LoginPage.vue'
import ResultPage from './views/ResultPage.vue'

const loading = ref(true)
const logoutLoading = ref(false)
const isAuthenticated = ref(false)
const userEmail = ref<string | null | undefined>(null)

onMounted(async () => {
  try {
    const user = await authClient.currentUser()
    isAuthenticated.value = user.isAuthenticated === true
    userEmail.value = user.email
  } finally {
    loading.value = false
  }
})

function onAuthenticated(email: string | null | undefined) {
  userEmail.value = email
  isAuthenticated.value = true
}

async function logout() {
  try {
    logoutLoading.value = true
    await authClient.logout()
    isAuthenticated.value = false
    userEmail.value = null
  } finally {
    logoutLoading.value = false
  }
}
</script>

<style lang="scss">
.v-navigation-drawer .v-ripple__container {
  display: none !important;
}

html {
  overflow-y: auto !important;
}

pre {
  white-space: pre-wrap;
}

.app-loading {
  align-items: center;
  display: flex;
  height: 100%;
  justify-content: center;
}

.app-shell {
  display: flex;
  flex-direction: column;
  height: 100%;
  width: 100%;
}

.app-header {
  align-items: center;
  border-bottom: 1px solid #dde3eb;
  display: flex;
  flex: 0 0 52px;
  justify-content: flex-end;
  padding: 0 16px;
}

.app-content {
  flex: 1 1 auto;
  min-height: 0;
}

.user-email {
  color: #45505f;
  font-size: 14px;
  margin-right: 12px;
}
</style>
