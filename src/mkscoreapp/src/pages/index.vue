<template>
  <v-card class="mx-auto" max-width="448">
    <v-layout>
      <v-app-bar density="compact">
        <template v-slot:prepend>
          <v-app-bar-nav-icon></v-app-bar-nav-icon>
        </template>

        <v-app-bar-title>MkScore</v-app-bar-title>

        <v-spacer></v-spacer>

        <v-btn icon @click="dialog = !dialog">
          <v-icon>mdi-plus</v-icon>
        </v-btn>

        <v-btn icon>
          <v-icon>mdi-dots-vertical</v-icon>
        </v-btn>
      </v-app-bar>

      <v-main>
        <v-container fluid>
          <v-dialog v-model="dialog" persistent>
            <AddDialog />
          </v-dialog>

          <Top class="mt-1" />
        </v-container>
      </v-main>
    </v-layout>
  </v-card>
</template>

<script lang="ts" setup>
const dialog = ref(false);
</script>
