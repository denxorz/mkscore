<template>
  <div class="lists-container mx-2">
    <v-card class="list">
      <v-card-title>Top 3 all time</v-card-title>
      <v-list lines="two">
        <v-list-item
          v-for="score in top3AllTime"
          :key="score.position"
          :title="score.player"
          :subtitle="`${score.totalRaces} races`"
        >
          <template v-slot:append>
            <v-chip color="success" variant="tonal" class="score-chip">
              {{ score.avgScore }}
            </v-chip>
          </template>
        </v-list-item>
      </v-list>
    </v-card>

    <v-card class="list">
      <v-card-title>Top 3 this week</v-card-title>
      <v-list lines="two">
        <v-list-item
          v-for="score in top3ThisWeek"
          :key="score.position"
          :title="score.player"
          :subtitle="`${score.totalRaces} races`"
        >
          <template v-slot:append>
            <v-chip color="success" variant="tonal" class="score-chip">
              {{ score.avgScore }}
            </v-chip>
          </template>
        </v-list-item>
      </v-list>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { useQuery } from "@vue/apollo-composable";
import { graphql } from "@/gql";
import { DateTime } from "luxon";
import { groupBy, orderBy, sumBy, map, ceil } from "lodash";

const { result, loading } = useQuery(
  graphql(`
    query scores {
      scores {
        id
        position
        name
        score
        isHuman
        player
      }
    }
  `)
);

const allTime = computed(() => result.value?.scores?.filter((s) => !!s) ?? []);

const startOfWeek = DateTime.now().startOf("week");
const idFilterThisWeek = `human_${startOfWeek.toISODate()}`;
const thisWeek = computed(() =>
  allTime.value.filter((s) => s.id > idFilterThisWeek)
);

const byPlayerAllTime = computed(() =>
  groupBy(allTime.value, (s) => s.player ?? "")
);
const byPlayerScoreAllTime = computed(() =>
  map(byPlayerAllTime.value, (p) => ({
    player: p[0].player ?? "??",
    avgScore: ceil(sumBy(p, (s) => s.score ?? 0) / p.length),
    totalRaces: p.length,
  }))
);
const byScoreAllTime = computed(() =>
  orderBy(
    byPlayerScoreAllTime.value.filter((p) => p.player),
    ["avgScore"],
    ["desc"]
  )
);
const top3AllTime = computed(() =>
  byScoreAllTime.value.slice(0, 3).map((s, i) => ({ ...s, position: i + 1 }))
);

const byPlayerThisWeek = computed(() =>
  groupBy(thisWeek.value, (s) => s.player ?? "")
);
const byPlayerScoreThisWeek = computed(() =>
  map(byPlayerThisWeek.value, (p) => ({
    player: p[0].player ?? "??",
    avgScore: ceil(sumBy(p, (s) => s.score ?? 0) / p.length),
    totalRaces: p.length,
  }))
);
const byScoreThisWeek = computed(() =>
  orderBy(
    byPlayerScoreThisWeek.value.filter((p) => p.player),
    ["avgScore"],
    ["desc"]
  )
);
const top3ThisWeek = computed(() =>
  byScoreThisWeek.value.slice(0, 3).map((s, i) => ({ ...s, position: i + 1 }))
);
</script>

<style scoped>
.lists-container {
  display: flex;
  gap: 1rem;
  justify-content: space-between;
}

.list {
  flex: 1;
}

.score-chip {
  font-weight: 600;
}

@media (max-width: 640px) {
  .lists-container {
    flex-direction: column;
  }
}
</style>
