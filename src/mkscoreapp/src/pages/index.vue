<template>
  <v-dialog v-model="dialog" persistent>
    <HelloWorld />
  </v-dialog>

  <v-fab class="ms-4" icon="mdi-plus"
    @click="dialog = !dialog"></v-fab>
</template>

<script lang="ts" setup>
const dialog = ref(false)
</script>
