
import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
  overwrite: true,
  schema: [
    {
      'https://pgagcjxksvdqbkkfwjdszvag7u.appsync-api.eu-central-1.amazonaws.com/graphql': {
        headers: {
          "x-api-key": process.env.GraphQLAPIKey,
        },
      },
    },
  ],
  documents: "src/**/*.vue",
  ignoreNoDocuments: true,
  generates: {
    "src/gql/": {
      preset: "client",
      plugins: [],
      config: {
        useTypeImports: true
      }
    },
    "./graphql.schema.json": {
      plugins: ["introspection"]
    }
  }
};

export default config;
