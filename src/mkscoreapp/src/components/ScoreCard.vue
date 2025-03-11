<template>
  <v-card class="list" density="compact" variant="text" border>
    <v-card-title>{{ title }}</v-card-title>
    <v-list lines="two">
      <v-list-item v-for="score in scores" :key="score.position">
        <template v-slot:prepend>
          <v-chip color="primary" class="position-chip">
            {{ score.position }}
          </v-chip>
        </template>
        <template v-slot:title>
          <div class="player-info">
            {{ score.player }}
          </div>
        </template>
        <template v-slot:subtitle> {{ score.totalRaces }} races </template>
        <template v-slot:append>
          <v-chip color="success" variant="tonal" class="score-chip">
            {{ score.avgScore }}
          </v-chip>
        </template>
      </v-list-item>
    </v-list>
  </v-card>
</template>

<script setup lang="ts">
import { defineProps } from "vue";

defineProps<{
  title: string;
  scores: Array<{
    position: number;
    player: string;
    totalRaces: number;
    avgScore: number;
  }>;
}>();
</script>

<style scoped>
.list {
  flex: 1;
  margin: 1rem;
}

.position-chip {
  font-weight: 600;
  font-size: 1.2rem;
  margin-right: 1rem;
}

.player-info {
  display: flex;
  align-items: center;
}

.score-chip {
  font-weight: 600;
}
</style>
