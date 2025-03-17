
import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
  overwrite: true,
  schema: [
    {
      'https://j27amiviy5b3lbmimtqnskwjta.appsync-api.eu-west-1.amazonaws.com/graphql': {
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
