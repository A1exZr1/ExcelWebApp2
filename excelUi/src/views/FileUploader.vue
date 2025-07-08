<template>
  <v-card flat class="pa-0">
    <v-file-input
      v-model="selectedFile"
      :label="props.label"
      accept=".xlsx, .xls"
      prepend-icon="mdi-file-excel"
      class="mb-2"
      hide-details
    />

    <v-btn
      color="primary"
      :disabled="!selectedFile"
      @click="upload"
      block
    >
      {{ props.label }}
    </v-btn>

    <v-alert
      v-if="message"
      type="info"
      class="mt-2"
      dense
    >
      {{ message }}
    </v-alert>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'


const props = defineProps({
  label: String,
  endpoint: String
});

// Emits
const emit = defineEmits(['uploaded'])

const selectedFile = ref<File | null>(null)
const message = ref('')

async function upload() {
  if (!selectedFile.value) return

  const formData = new FormData()
  formData.append('file', selectedFile.value)

  try {
    console.log(props.endpoint);
    

    const response = await axios.post(props.endpoint!, formData)
    const { count, message: serverMessage } = response.data
    message.value = `Загружено ${count} строк. ${serverMessage ?? ''}`
    emit('uploaded')
  } catch (error: any) {
    message.value = `Ошибка: ${error?.response?.data || error.message}`
  }
}
</script>
