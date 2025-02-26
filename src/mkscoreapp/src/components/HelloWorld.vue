<template>
  <v-container class="fill-height">
    <v-responsive class="align-centerfill-height mx-auto" max-width="900">
      <form @submit.prevent="handleSubmit">
        <input type="file" @change="handleChange">
        <button>add</button>
      </form>
    </v-responsive>
  </v-container>
</template>

<script setup lang="ts">
import { useQuery } from '@vue/apollo-composable'
import { graphql } from '@/gql'

const { result } = useQuery(
  graphql(/* GraphQL */`
    query getJob($id: ID!) {
      job(id: $id) {
        name
      }
    }
  `),
  { id: "1337" }
)

const films = computed(() => result.value)

const file = ref();

function handleChange(e: any) {
  file.value = e.target.files[0];
}

async function handleSubmit() {
  //await uploadImage(file.value);
}


// async function uploadImage(file:any) {
//   try {
//     const url = await axios.get(
//       "https://yw9x82kgmg.execute-api.eu-central-1.amazonaws.com/prod/getUploadUrl", {
//       headers: {
//         "Access-Control-Allow-Origin": "*",
//       }
//     })
//     console.log({ url });

//     await axios.put(url.data.preSignedUrl, file, {
//       headers: {
//         'Content-Type': file.type,
//         "Access-Control-Allow-Origin": "*",
//       }
//     });
//   } catch (err) {
//     console.log(err.message);
//   }
// }
</script>
