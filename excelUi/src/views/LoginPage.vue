<template>
  <div class="login-page">
    <v-card class="login-card" elevation="3">
      <v-card-title>Вход</v-card-title>
      <v-card-text>
        <v-form @submit.prevent="submit">
          <v-text-field
            v-model="email"
            label="Email"
            type="email"
            variant="solo-filled"
            density="comfortable"
            autocomplete="username"
            :disabled="loading"
          />
          <v-text-field
            v-model="password"
            label="Пароль"
            type="password"
            variant="solo-filled"
            density="comfortable"
            autocomplete="current-password"
            :disabled="loading"
          />
          <v-checkbox
            v-model="rememberMe"
            label="Запомнить меня"
            density="compact"
            :disabled="loading"
          />
          <v-alert v-if="error" type="error" density="compact" class="mb-4">
            {{ error }}
          </v-alert>
          <v-btn
            type="submit"
            color="primary"
            block
            size="large"
            :loading="loading"
            :disabled="!email || !password"
          >
            Войти
          </v-btn>
        </v-form>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { authClient, getApiErrorMessage } from '../api/fileReaderApi'

const emit = defineEmits<{
  authenticated: [email: string | null | undefined]
}>()

const email = ref('')
const password = ref('')
const rememberMe = ref(true)
const loading = ref(false)
const error = ref('')

async function submit() {
  try {
    loading.value = true
    error.value = ''
    const user = await authClient.login({
      email: email.value,
      password: password.value,
      rememberMe: rememberMe.value,
    })
    emit('authenticated', user.email)
  } catch (err) {
    error.value = getApiErrorMessage(err)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  align-items: center;
  background: #f6f8fb;
  display: flex;
  height: 100%;
  justify-content: center;
  padding: 24px;
  width: 100%;
}

.login-card {
  border-radius: 8px;
  max-width: 420px;
  width: 100%;
}
</style>
