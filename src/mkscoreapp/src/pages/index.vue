<template>
  <AddDialog v-model:isOpen="isAddDialogOpen" />
  <v-card class="mx-auto" max-width="448">
    <v-layout>
      <v-app-bar density="compact">
        <template v-slot:prepend>
          <v-app-bar-nav-icon></v-app-bar-nav-icon>
        </template>

        <v-app-bar-title>MkScore</v-app-bar-title>

        <v-spacer></v-spacer>

        <v-btn icon @click="isAddDialogOpen = !isAddDialogOpen">
          <v-icon>mdi-plus</v-icon>
        </v-btn>

        <v-btn icon>
          <v-icon>mdi-dots-vertical</v-icon>
        </v-btn>

        <v-btn @click="loginWithRedirect">Log in</v-btn>
        <v-btn @click="logout">Log out</v-btn>
      </v-app-bar>

      <v-main>
        <v-container fluid>
          <Top class="mt-1" />
        </v-container>
      </v-main>
    </v-layout>
  </v-card>
</template>

<script lang="ts" setup>
import { useAuth0 } from "@auth0/auth0-vue";

const isAddDialogOpen = ref(false);
const { loginWithRedirect, logout } = useAuth0();
</script>
