<template>
  <v-container class="fill-height">
    <v-responsive class="align-centerfill-height mx-auto" max-width="900">
      <v-card>
        <v-progress-linear v-show="isWorking" color="deep-purple-accent-4" height="6" indeterminate
          rounded></v-progress-linear>
        <v-form @submit.prevent="handleSubmit">
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
import type { Job } from '@/gql/graphql';

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
