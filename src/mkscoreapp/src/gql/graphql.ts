/* eslint-disable */
import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
};

export type CreateJobInput = {
  name?: InputMaybe<Scalars['String']['input']>;
};

export type Job = {
  __typename?: 'Job';
  id: Scalars['ID']['output'];
  isFinished?: Maybe<Scalars['Boolean']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  scores?: Maybe<Array<Maybe<ScoreEntry>>>;
  uploadUrl?: Maybe<Scalars['String']['output']>;
};

export type Mutation = {
  __typename?: 'Mutation';
  createJob?: Maybe<Job>;
  updateJob?: Maybe<Job>;
};


export type MutationCreateJobArgs = {
  input: CreateJobInput;
};


export type MutationUpdateJobArgs = {
  input: UpdateJobInput;
};

export type Query = {
  __typename?: 'Query';
  job?: Maybe<Job>;
};


export type QueryJobArgs = {
  id: Scalars['ID']['input'];
};

export type ScoreEntry = {
  __typename?: 'ScoreEntry';
  isHuman?: Maybe<Scalars['Boolean']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  position?: Maybe<Scalars['Int']['output']>;
  score?: Maybe<Scalars['Int']['output']>;
};

export type ScoreEntryInput = {
  isHuman?: InputMaybe<Scalars['Boolean']['input']>;
  name?: InputMaybe<Scalars['String']['input']>;
  position?: InputMaybe<Scalars['Int']['input']>;
  score?: InputMaybe<Scalars['Int']['input']>;
};

export type Subscription = {
  __typename?: 'Subscription';
  updatedJob?: Maybe<Job>;
};


export type SubscriptionUpdatedJobArgs = {
  id: Scalars['ID']['input'];
};

export type UpdateJobInput = {
  id?: InputMaybe<Scalars['ID']['input']>;
  isFinished?: InputMaybe<Scalars['Boolean']['input']>;
  scores?: InputMaybe<Array<InputMaybe<ScoreEntryInput>>>;
  uploadUrl?: InputMaybe<Scalars['String']['input']>;
};

export type CreateJobMutationVariables = Exact<{
  input: CreateJobInput;
}>;


export type CreateJobMutation = { __typename?: 'Mutation', createJob?: { __typename?: 'Job', id: string, uploadUrl?: string | null } | null };

export type UpdatedJobSubscriptionVariables = Exact<{
  id: Scalars['ID']['input'];
}>;


export type UpdatedJobSubscription = { __typename?: 'Subscription', updatedJob?: { __typename?: 'Job', id: string, isFinished?: boolean | null, scores?: Array<{ __typename?: 'ScoreEntry', position?: number | null, name?: string | null, score?: number | null, isHuman?: boolean | null } | null> | null } | null };


export const CreateJobDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"createJob"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateJobInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createJob"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"uploadUrl"}}]}}]}}]} as unknown as DocumentNode<CreateJobMutation, CreateJobMutationVariables>;
export const UpdatedJobDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"updatedJob"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updatedJob"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFinished"}},{"kind":"Field","name":{"kind":"Name","value":"scores"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"position"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"score"}},{"kind":"Field","name":{"kind":"Name","value":"isHuman"}}]}}]}}]}}]} as unknown as DocumentNode<UpdatedJobSubscription, UpdatedJobSubscriptionVariables>;