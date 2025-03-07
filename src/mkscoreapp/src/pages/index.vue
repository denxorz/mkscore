<template>
  <v-dialog v-model="dialog" persistent>
    <AddDialog />
  </v-dialog>
  <v-fab class="ms-4" icon="mdi-plus" @click="dialog = !dialog" location="right top" position="absolute"></v-fab>

  <Top />
</template>

<script lang="ts" setup>
const dialog = ref(false)
</script>
