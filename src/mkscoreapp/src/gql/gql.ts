/* eslint-disable */
import * as types from './graphql';
import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';

/**
 * Map of all GraphQL operations in the project.
 *
 * This map has several performance disadvantages:
 * 1. It is not tree-shakeable, so it will include all operations in the project.
 * 2. It is not minifiable, so the string of a GraphQL query will be multiple times inside the bundle.
 * 3. It does not support dead code elimination, so it will add unused operations.
 *
 * Therefore it is highly recommended to use the babel or swc plugin for production.
 * Learn more about it here: https://the-guild.dev/graphql/codegen/plugins/presets/preset-client#reducing-bundle-size
 */
type Documents = {
    "\n    mutation createJob($input: CreateJobInput!) {\n      createJob(input: $input) {\n        id\n        uploadUrl\n      }\n    }\n  ": typeof types.CreateJobDocument,
    "\n    mutation createScore($input: CreateScoreInput!) {\n      createScore(input: $input) {\n        id\n      }\n    }\n  ": typeof types.CreateScoreDocument,
    "\n      subscription updatedJob($id: ID!) {\n        updatedJob(id: $id) {\n          id\n          isFinished\n          scores {\n            position\n            name\n            score\n            isHuman\n          }\n        }\n      }\n    ": typeof types.UpdatedJobDocument,
};
const documents: Documents = {
    "\n    mutation createJob($input: CreateJobInput!) {\n      createJob(input: $input) {\n        id\n        uploadUrl\n      }\n    }\n  ": types.CreateJobDocument,
    "\n    mutation createScore($input: CreateScoreInput!) {\n      createScore(input: $input) {\n        id\n      }\n    }\n  ": types.CreateScoreDocument,
    "\n      subscription updatedJob($id: ID!) {\n        updatedJob(id: $id) {\n          id\n          isFinished\n          scores {\n            position\n            name\n            score\n            isHuman\n          }\n        }\n      }\n    ": types.UpdatedJobDocument,
};

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 *
 *
 * @example
 * ```ts
 * const query = graphql(`query GetUser($id: ID!) { user(id: $id) { name } }`);
 * ```
 *
 * The query argument is unknown!
 * Please regenerate the types.
 */
export function graphql(source: string): unknown;

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    mutation createJob($input: CreateJobInput!) {\n      createJob(input: $input) {\n        id\n        uploadUrl\n      }\n    }\n  "): (typeof documents)["\n    mutation createJob($input: CreateJobInput!) {\n      createJob(input: $input) {\n        id\n        uploadUrl\n      }\n    }\n  "];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n    mutation createScore($input: CreateScoreInput!) {\n      createScore(input: $input) {\n        id\n      }\n    }\n  "): (typeof documents)["\n    mutation createScore($input: CreateScoreInput!) {\n      createScore(input: $input) {\n        id\n      }\n    }\n  "];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n      subscription updatedJob($id: ID!) {\n        updatedJob(id: $id) {\n          id\n          isFinished\n          scores {\n            position\n            name\n            score\n            isHuman\n          }\n        }\n      }\n    "): (typeof documents)["\n      subscription updatedJob($id: ID!) {\n        updatedJob(id: $id) {\n          id\n          isFinished\n          scores {\n            position\n            name\n            score\n            isHuman\n          }\n        }\n      }\n    "];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;