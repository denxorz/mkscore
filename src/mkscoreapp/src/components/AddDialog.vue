<template>
  <v-dialog
    v-model="isOpen"
    fullscreen
    transition="dialog-bottom-transition"
    @open="resetDialog"
  >
    <v-card>
      <v-toolbar>
        <v-btn
          icon="mdi-close"
          @click="handleCancel"
        />

        <v-toolbar-title>Add</v-toolbar-title>

        <v-toolbar-items>
          <v-btn
            text="Submit scores"
            variant="text"
            :disabled="isWorking || !file"
            @click="submitScores"
          />
        </v-toolbar-items>
      </v-toolbar>

      <v-img
        v-if="image"
        :src="image"
        max-height="300"
      />

      <v-progress-linear
        v-show="isWorking"
        indeterminate
        rounded
      />

      <v-date-picker
        v-model="date"
        show-adjacent-months
      />

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
          <VInlineCustomField
            v-model="item.score"
            :loading-wait="false"
          >
            <template #default>
              <div class="slider-container">
                <v-slider
                  v-model="item.score"
                  max="60"
                  :step="1"
                  hide-details
                  class="styled-slider"
                />
                <v-text-field
                  v-model="item.score"
                  density="compact"
                  style="width: 80px"
                  type="number"
                  variant="outlined"
                  hide-details
                />
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
        v-if="!items?.length"
        class="styled-form"
        @submit.prevent="addNewImage"
      >
        <v-file-input
          v-model="file"
          :disabled="isWorking"
        />
        <v-btn
          icon
          :disabled="isWorking"
          @click="openCamera"
        >
          <v-icon>mdi-camera</v-icon>
        </v-btn>
      </v-form>

      <v-btn
        v-if="!items?.length"
        type="submit"
        :disabled="isWorking || !file"
        @click="addNewImage"
      >
        Add
      </v-btn>
      <v-btn
        v-else
        :disabled="isWorking || !file"
        @click="submitScores"
      >
        Submit scores
      </v-btn>
      <v-btn @click="handleCancel">
        Cancel
      </v-btn>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { useMutation, useSubscription } from "@vue/apollo-composable";
import { graphql } from "@/gql";
import axios from "axios";
import type { Job, Maybe, ScoreSuggestion } from "@/gql/graphql";
import { DateTime } from "luxon";

type ScoreSuggestionL = {
  isHuman?: boolean | null;
  name?: string | null;
  position?: number | null;
  score?: number;
  player?: string;
};

const isOpen = defineModel<boolean>("isOpen");

const date = ref(DateTime.now().toJSDate());
const job = ref<Job | null | undefined>();
const image = computed(() => job.value?.imageUrl);
const id = computed(() => job.value?.id || "");
const isWorking = ref(false);
const items = ref<ScoreSuggestionL[]>([]);
const headers = [
  { title: "Pos", key: "position", width: "100px" },
  { title: "Name", key: "name", width: "200px" },
  { title: "Score", key: "score", width: "300px" },
  { title: "Player", key: "player", width: "150px" },
];

watch(image, (img) => console.log({ img }));

const { mutate: createJob } = useMutation(
  graphql(`
    mutation createJob($input: CreateJobInput!) {
      createJob(input: $input) {
        id
        uploadUrl
        imageUrl
      }
    }
  `)
);

const { mutate: createScore } = useMutation(
  graphql(`
    mutation createScore($input: CreateScoreInput!) {
      createScore(input: $input) {
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

const playerName = (s: Maybe<ScoreSuggestion>) => {
  if (!s?.isHuman) return undefined;

  switch (s.name) {
    case "Waluigi":
      return "JP";
    case "Diddy Kong":
      return "Koen";
    case "Yoshi":
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
    case "Isabelle":
      return "Ploy";
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
        imageUrl
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
  } catch (err: unknown) {
    console.log(err);
  }
}

async function submitScores() {
  isWorking.value = true;
  const isoDate = DateTime.fromJSDate(date.value).toISODate();

  const scores = items.value.map((score) => ({
    id: `${score.isHuman ? "human" : "cpu"}_${isoDate}_${score.position}`,
    jobId: job.value?.id,
    date: isoDate,
    name: score.player,
    isHuman: score.isHuman,
    position: score.position,
    player: score.player,
    score: score.score,
  }));

  try {
    await Promise.all(scores.map((s) => createScore({ input: s })));
  } catch (error) {
    console.error("Error submitting scores:", error);
  } finally {
    isWorking.value = false;
    isOpen.value = false;
  }
}

function resetDialog() {
  job.value = null;
  isWorking.value = false;
  items.value = [];
  file.value = undefined;
}

function handleCancel() {
  resetDialog();
  isOpen.value = false;
}

function openCamera() {
  const input = document.createElement("input");
  input.type = "file";
  input.accept = "image/*";
  input.capture = "environment";
  input.onchange = (event: Event) => {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files[0]) {
      file.value = target.files[0];
    }
  };
  input.click();
}

watch(file, nv => {
  if (!nv) return;

  const switchFilenameDate = /^(?<date>\d{16})_c.jpg$/g.exec(nv.name)?.groups?.date;
  if (switchFilenameDate && switchFilenameDate.length > 7) {
    date.value = DateTime.fromISO(switchFilenameDate.slice(0, 8)).toJSDate()!;
  }
})
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

.v-file-input {
  margin-top: 20px;
}
</style>
