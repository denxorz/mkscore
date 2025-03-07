<template>
  <v-container class="fill-height">
    <v-responsive class="align-centerfill-height mx-auto" max-width="900">
      <v-card class="mx-auto" max-width="300">
        <v-progress-linear v-show="loading" indeterminate rounded>
        </v-progress-linear>
        <v-data-table :items="top3AllTime" hide-default-footer hide-default-header :headers="headers">
        </v-data-table>
        <v-data-table :items="top3ThisWeek" hide-default-footer hide-default-header :headers="headers">
        </v-data-table>
      </v-card>
    </v-responsive>
  </v-container>
</template>

<script setup lang="ts">
import { useQuery } from "@vue/apollo-composable";
import { graphql } from "@/gql";
import { DateTime } from 'luxon';
import type { Score } from "@/gql/graphql";
import { groupBy, mapValues, orderBy, sumBy, map, ceil } from 'lodash'

const headers = [
  { title: "", key: "position" },
  { title: "Player", key: "player" },
  { title: "Score", key: "avgScore" },
  { title: "Races", key: "totalRaces" },
];

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
      }`));

const allTime = computed(() => result.value?.scores?.filter(s => !!s) ?? []);

const startOfWeek = DateTime.now().startOf('week');
const idFilterThisWeek = `human_${startOfWeek.toISODate()}`;
const thisWeek = computed(() => allTime.value.filter((s) => s.id > idFilterThisWeek));

const byPlayerAllTime = computed(() => groupBy(allTime.value, s => s.player ?? ''));
const byPlayerScoreAllTime = computed(() => map(byPlayerAllTime.value, (p) => ({
  player: p[0].player,
  avgScore: ceil(sumBy(p, s => s.score ?? 0) / p.length),
  totalRaces: p.length
})));
const byScoreAllTime = computed(() => orderBy(byPlayerScoreAllTime.value.filter(p => p.player), ['avgScore'], ['desc']));
const top3AllTime = computed(() => byScoreAllTime.value.slice(0, 5).map((s, i) => ({ ...s, position: i + 1 })));

const byPlayerThisWeek = computed(() => groupBy(thisWeek.value, s => s.player ?? ''));
const byPlayerScoreThisWeek = computed(() => map(byPlayerThisWeek.value, (p) => ({
  player: p[0].player,
  avgScore: ceil(sumBy(p, s => s.score ?? 0) / p.length),
  totalRaces: p.length
})));
const byScoreThisWeek = computed(() => orderBy(byPlayerScoreThisWeek.value.filter(p => p.player), ['avgScore'], ['desc']));
const top3ThisWeek = computed(() => byScoreThisWeek.value.slice(0, 5).map((s, i) => ({ ...s, position: i + 1 })));

</script>
