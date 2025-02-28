<template>
  <v-container class="fill-height">
    <v-responsive class="align-centerfill-height mx-auto" max-width="900">
      <v-card class="mx-auto" max-width="300">
        <v-progress-linear v-show="isWorking" indeterminate rounded></v-progress-linear>

        <v-data-table :items="items" hide-default-footer hide-default-header :headers="headers">
          <template v-slot:item.player="{ value }">
            <v-chip v-if="value">
              {{ value }}
            </v-chip>
          </template>
        </v-data-table>

        <v-form @submit.prevent="handleSubmit" v-if="!items?.length">
          <v-file-input v-model="file" :disabled="isWorking" />
          <v-btn type="submit" :disabled="isWorking">add</v-btn>
        </v-form>
      </v-card>
    </v-responsive>
  </v-container>
</template>

<script setup lang="ts">
import { useMutation, useSubscription } from '@vue/apollo-composable'
import { graphql } from '@/gql'
import axios from 'axios'
import type { Job, Maybe, ScoreEntry } from '@/gql/graphql';

// const { result: queryResult } = useQuery(
//   graphql(`
//     query getJob($id: ID!) {
//       job(id: $id) {
//         name
//       }
//     }
//   `),
//   { id: "1337" }
// )

const job = ref<Job | null | undefined>();
const id = computed(() => job.value?.id || '');
const isWorking = ref(false);
const items = computed(() => (job?.value?.scores ?? []).map(s => ({ ...s, player: playerName(s) })))
const headers = [
  { title: 'Pos', key: 'position' },
  { title: 'Name', key: 'name' },
  { title: 'Score', key: 'score' },
  { title: 'Player', key: 'player' },
]

const { mutate: createJob } = useMutation(
  graphql(`
    mutation createJob($input: CreateJobInput!) {
      createJob(input: $input) {
        id
        uploadUrl
      }
    }
  `)
)

const playerName = (s: Maybe<ScoreEntry>) => {
  if (!s?.isHuman) return undefined;

  switch (s.name) {
    case "Waluigi": return "JP";
    case "Diddy Kong": return "Koen";
    case "Donkey Kong": return "Marcel";
    case "Peach": return "Lui";
    case "Roy": return "Wim";
    case "Dry Bones": return "Dennis";
    case "Baby Mario": return "Boris";
    default: return undefined;
  }
};

const subscriptionVariables = computed(() => ({ id: id.value }));
const subscriptionEnabled = computed(() => !!(subscriptionVariables.value?.id));
const { result: subscriptionResult, error: createSubError } = useSubscription(
  graphql(`
      subscription updatedJob($id: ID!) {
        updatedJob(id: $id) {
          id
          isFinished
          scores {
            position
            name
            score
            isHuman
          }
        }
      }
    `),
  subscriptionVariables,
  () => ({
    enabled: subscriptionEnabled.value
  })
)
watch(subscriptionResult, nv => {
  job.value = nv?.updatedJob;
  isWorking.value = !(job.value?.isFinished ?? false);
});
watch(createSubError, nv => console.log("subscriptionError", { nv }));

const file = ref<File>();

async function handleSubmit() {
  if (!file.value) return;

  isWorking.value = true;
  const res = await createJob({ input: { name: "test" } });
  job.value = res?.data?.createJob;

  await uploadImage(file.value, res?.data?.createJob?.uploadUrl ?? '');
}

async function uploadImage(file: File, url: string) {
  try {
    await axios.put(url, file, {
      headers: {
        'Content-Type': file.type,
        "Access-Control-Allow-Origin": "*",
      }
    });
  } catch (err: any) {
    console.log(err.message);
  }
}
</script>
