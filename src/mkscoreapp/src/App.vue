<script setup>
import HelloWorld from './components/HelloWorld.vue'
import { ref } from 'vue'
import axios from 'axios'

// const formData = new FormData();
// Object.entries(fields).forEach(([key, value]) => {
//     formData.append(key, value);
// });

// formData.append('file', file.originFileObj);

// axios.post(url, formData, {
//     headers: {
//         'Content-Type': 'multipart/form-data',
//     },
// })

const file = ref();

function handleChange(e) {
  file.value = e.target.files[0];
}

async function handleSubmit() {
  await uploadImage(file.value);
}


async function uploadImage(file) {
  try {
    const url = await axios.get(
      "https://yw9x82kgmg.execute-api.eu-central-1.amazonaws.com/prod/getUploadUrl", {
        headers: {
          "Access-Control-Allow-Origin": "*",
        }
    })
    console.log({ url });

    await axios.put(url.data.preSignedUrl, file, {
      headers: {
        'Content-Type': file.type,
        "Access-Control-Allow-Origin": "*",
      }
    });
  } catch (err) {
    console.log(err.message);
  }
}
</script>

<template>
  <form @submit.prevent="handleSubmit">
    <input type="file" @change="handleChange">
    <button>add</button>
  </form>
</template>

<style scoped></style>
