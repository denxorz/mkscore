<template>
  <div class="lists-container">
    <ScoreCard title="Top 3 all time" :scores="top3AllTime" />
    <ScoreCard title="Top 3 this week" :scores="top3ThisWeek" />
  </div>
</template>

<script setup lang="ts">
import { useQuery } from "@vue/apollo-composable";
import { DateTime } from "luxon";
import { groupBy, orderBy, sumBy, map, ceil } from "lodash";

import { graphql } from "@/gql";

import ScoreCard from "@/components/ScoreCard.vue";

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
  justify-content: space-between;
}

@media (max-width: 640px) {
  .lists-container {
    flex-direction: column;
  }
}
</style>
