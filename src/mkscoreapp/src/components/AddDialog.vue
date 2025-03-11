<template>
  <v-dialog v-model="isOpen" persistent fullscreen>
    <v-card>
      <v-progress-linear
        v-show="isWorking"
        indeterminate
        rounded
      ></v-progress-linear>

      <v-data-table
        v-if="items?.length"
        :items="items"
        hide-default-footer
        hide-default-header
        :headers="headers"
        class="styled-table"
        width="100%"
      >
        <template #[`item.score`]="{ item }">
          <VInlineCustomField v-model="item.score" :loading-wait="false">
            <template #default="settings">
              <div class="slider-container">
                <v-slider
                  v-model="item.score"
                  max="60"
                  :step="1"
                  hide-details
                  class="styled-slider"
                ></v-slider>
                <v-text-field
                  v-model="item.score"
                  density="compact"
                  style="width: 80px"
                  type="number"
                  variant="outlined"
                  hide-details
                ></v-text-field>
              </div>
            </template>
          </VInlineCustomField>
        </template>
        <template #[`item.player`]="{ item }">
          <VInlineSelect
            v-model="item.player"
            :items="players"
            name="state"
            :loading-wait="false"
          />
        </template>
      </v-data-table>

      <v-form
        @submit.prevent="addNewImage"
        v-if="!items?.length"
        class="styled-form"
      >
        <v-file-input v-model="file" :disabled="isWorking" />
      </v-form>

      <v-btn
        v-if="!items?.length"
        type="submit"
        :disabled="isWorking"
        @click="addNewImage"
        >Add</v-btn
      >
      <v-btn v-else :disabled="isWorking || !file" @click="submitScores"
        >Submit scores</v-btn
      >
      <v-btn @click="isOpen = false">Cancel</v-btn>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { useMutation, useSubscription } from "@vue/apollo-composable";
import { graphql } from "@/gql";
import axios from "axios";
import type { Job, Maybe, ScoreSuggestion } from "@/gql/graphql";

type ScoreSuggestionL = {
  isHuman?: boolean | null;
  name?: string | null;
  position?: number | null;
  score?: number;
  player?: string;
};

const isOpen = defineModel<boolean>("isOpen");

const job = ref<Job | null | undefined>();
const id = computed(() => job.value?.id || "");
const isWorking = ref(false);
const items = ref<ScoreSuggestionL[]>([]);
const headers = [
  { title: "Pos", key: "position", width: "100px" },
  { title: "Name", key: "name", width: "200px" },
  { title: "Score", key: "score", width: "300px" },
  { title: "Player", key: "player", width: "150px" },
];

const { mutate: createJob } = useMutation(
  graphql(`
    mutation createJob($input: CreateJobInput!) {
      createJob(input: $input) {
        id
        uploadUrl
      }
    }
  `)
);

const { mutate: createScore } = useMutation(
  graphql(`
    mutation createScore($input: CreateScoreInput!) {
      createScore(input: $input) {
        id
      }
    }
  `)
);

const playerName = (s: Maybe<ScoreSuggestion>) => {
  if (!s?.isHuman) return undefined;

  switch (s.name) {
    case "Waluigi":
      return "JP";
    case "Diddy Kong":
      return "Koen";
    case "Donkey Kong":
      return "Marcel";
    case "Peach":
      return "Lui";
    case "Roy":
      return "Wim";
    case "Dry Bones":
      return "Dennis";
    case "Baby Mario":
      return "Boris";
    default:
      return undefined;
  }
};
const players = [
  "",
  "JP",
  "Koen",
  "Marcel",
  "Lui",
  "Wim",
  "Dennis",
  "Boris",
  "Ploy",
  "Erwin",
];

const subscriptionVariables = computed(() => ({ id: id.value }));
const subscriptionEnabled = computed(() => !!subscriptionVariables.value?.id);
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
    enabled: subscriptionEnabled.value,
  })
);
watch(subscriptionResult, (nv) => {
  job.value = nv?.updatedJob;
  isWorking.value = !(job.value?.isFinished ?? false);
  items.value = (job?.value?.scores ?? []).map((s) => ({
    ...s,
    player: playerName(s),
    score: s?.score ?? undefined,
  }));
});
watch(createSubError, (nv) => console.log("subscriptionError", { nv }));

const file = ref<File>();

async function addNewImage() {
  if (!file.value) return;

  isWorking.value = true;
  try {
    const res = await createJob({ input: { name: "test" } });
    job.value = res?.data?.createJob;

    await uploadImage(file.value, res?.data?.createJob?.uploadUrl ?? "");
  } catch (error) {
    console.error("Error adding new image:", error);
    isWorking.value = false;
  }
}

async function uploadImage(file: File, url: string) {
  try {
    await axios.put(url, file, {
      headers: {
        "Content-Type": file.type,
        "Access-Control-Allow-Origin": "*",
      },
    });
  } catch (err: any) {
    console.log(err.message);
  }
}

async function submitScores() {
  isWorking.value = true;
  const date = new Date().toISOString();

  try {
    await Promise.all(
      items.value.map((score) => {
        const s = {
          id: `${score.isHuman ? "human" : "cpu"}_${date}_${score.position}`,
          jobId: job.value?.id,
          date,
          name: score.player,
          isHuman: score.isHuman,
          position: score.position,
          player: score.player,
          score: score.score,
        };
        return createScore({ input: s });
      })
    );
  } catch (error) {
    console.error("Error submitting scores:", error);
  } finally {
    isWorking.value = false;
    isOpen.value = false;
  }
}
</script>

<style scoped>
.styled-table {
  margin-top: 20px;
  width: 100%;
  border-collapse: collapse;
}

.styled-form {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
  gap: 10px;
}

.slider-container {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
}

.styled-slider .v-input__control {
  height: 36px;
  flex-grow: 1;
}

.v-file-input,
.v-btn {
  margin-top: 20px;
}

.v-btn {
  background-color: #1976d2;
  color: white;
}

.v-btn:hover {
  background-color: #1565c0;
}

.v-dialog {
  max-width: 90%;
}

.v-card {
  padding: 20px;
}
</style>
