<template>
  <v-container fluid class="pa-4" style="width: 100vh; height: 100vh;">
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title>Test Page</v-card-title>
          <v-card-text>
            <v-btn color="primary" @click="onClick">Click Me</v-btn>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
function onClick() {
  alert('Button clicked!')
}
</script>

<style scoped>
</style>
