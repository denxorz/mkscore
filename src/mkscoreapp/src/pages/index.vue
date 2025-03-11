<template>
  <v-dialog v-model="dialog" persistent>
    <AddDialog />
  </v-dialog>
  <v-fab
    class="ma-1"
    icon="mdi-plus"
    @click="dialog = !dialog"
    location="top end"
    color="primary"
    absolute
  ></v-fab>

  <Top />
</template>

<script lang="ts" setup>
const dialog = ref(false);
</script>
