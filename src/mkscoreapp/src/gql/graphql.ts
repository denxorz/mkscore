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
  /** The `AWSDateTime` scalar type provided by AWS AppSync, represents a valid ***extended*** [ISO 8601 DateTime](https://en.wikipedia.org/wiki/ISO_8601#Combined_date_and_time_representations) string. In other words, this scalar type accepts datetime strings of the form `YYYY-MM-DDThh:mm:ss.SSSZ`.  The scalar can also accept "negative years" of the form `-YYYY` which correspond to years before `0000`. For example, "**-2017-01-01T00:00Z**" and "**-9999-01-01T00:00Z**" are both valid datetime strings.  The field after the two digit seconds field is a nanoseconds field. It can accept between 1 and 9 digits. So, for example, "**1970-01-01T12:00:00.2Z**", "**1970-01-01T12:00:00.277Z**" and "**1970-01-01T12:00:00.123456789Z**" are all valid datetime strings.  The seconds and nanoseconds fields are optional (the seconds field must be specified if the nanoseconds field is to be used).  The [time zone offset](https://en.wikipedia.org/wiki/ISO_8601#Time_zone_designators) is compulsory for this scalar. The time zone offset must either be `Z` (representing the UTC time zone) or be in the format `±hh:mm:ss`. The seconds field in the timezone offset will be considered valid even though it is not part of the ISO 8601 standard. */
  AWSDateTime: { input: any; output: any; }
};

export type CreateJobInput = {
  name?: InputMaybe<Scalars['String']['input']>;
};

export type CreateScoreInput = {
  date?: InputMaybe<Scalars['AWSDateTime']['input']>;
  id: Scalars['ID']['input'];
  isHuman?: InputMaybe<Scalars['Boolean']['input']>;
  jobId?: InputMaybe<Scalars['ID']['input']>;
  name?: InputMaybe<Scalars['String']['input']>;
  player?: InputMaybe<Scalars['String']['input']>;
  position?: InputMaybe<Scalars['Int']['input']>;
  score?: InputMaybe<Scalars['Int']['input']>;
};

export type Job = {
  __typename?: 'Job';
  id: Scalars['ID']['output'];
  imageUrl?: Maybe<Scalars['String']['output']>;
  isFinished?: Maybe<Scalars['Boolean']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  scores?: Maybe<Array<Maybe<ScoreSuggestion>>>;
  uploadUrl?: Maybe<Scalars['String']['output']>;
};

export type Mutation = {
  __typename?: 'Mutation';
  createJob?: Maybe<Job>;
  createScore?: Maybe<Score>;
  updateJob?: Maybe<Job>;
};


export type MutationCreateJobArgs = {
  input: CreateJobInput;
};


export type MutationCreateScoreArgs = {
  input: CreateScoreInput;
};


export type MutationUpdateJobArgs = {
  input: UpdateJobInput;
};

export type Query = {
  __typename?: 'Query';
  job?: Maybe<Job>;
  scores?: Maybe<Array<Maybe<Score>>>;
};


export type QueryJobArgs = {
  id: Scalars['ID']['input'];
};

export type Score = {
  __typename?: 'Score';
  date?: Maybe<Scalars['AWSDateTime']['output']>;
  id: Scalars['ID']['output'];
  isHuman?: Maybe<Scalars['Boolean']['output']>;
  jobId?: Maybe<Scalars['ID']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  player?: Maybe<Scalars['String']['output']>;
  position?: Maybe<Scalars['Int']['output']>;
  score?: Maybe<Scalars['Int']['output']>;
};

export type ScoreSuggestion = {
  __typename?: 'ScoreSuggestion';
  isHuman?: Maybe<Scalars['Boolean']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  position?: Maybe<Scalars['Int']['output']>;
  score?: Maybe<Scalars['Int']['output']>;
};

export type ScoreSuggestionInput = {
  isHuman?: InputMaybe<Scalars['Boolean']['input']>;
  name?: InputMaybe<Scalars['String']['input']>;
  position?: InputMaybe<Scalars['Int']['input']>;
  score?: InputMaybe<Scalars['Int']['input']>;
};

export type Subscription = {
  __typename?: 'Subscription';
  createdScore?: Maybe<Score>;
  updatedJob?: Maybe<Job>;
};


export type SubscriptionUpdatedJobArgs = {
  id: Scalars['ID']['input'];
};

export type UpdateJobInput = {
  id?: InputMaybe<Scalars['ID']['input']>;
  imageUrl?: InputMaybe<Scalars['String']['input']>;
  isFinished?: InputMaybe<Scalars['Boolean']['input']>;
  scores?: InputMaybe<Array<InputMaybe<ScoreSuggestionInput>>>;
  uploadUrl?: InputMaybe<Scalars['String']['input']>;
};

export type CreateJobMutationVariables = Exact<{
  input: CreateJobInput;
}>;


export type CreateJobMutation = { __typename?: 'Mutation', createJob?: { __typename?: 'Job', id: string, uploadUrl?: string | null, imageUrl?: string | null } | null };

export type CreateScoreMutationVariables = Exact<{
  input: CreateScoreInput;
}>;


export type CreateScoreMutation = { __typename?: 'Mutation', createScore?: { __typename?: 'Score', id: string, position?: number | null, name?: string | null, score?: number | null, isHuman?: boolean | null, player?: string | null } | null };

export type UpdatedJobSubscriptionVariables = Exact<{
  id: Scalars['ID']['input'];
}>;


export type UpdatedJobSubscription = { __typename?: 'Subscription', updatedJob?: { __typename?: 'Job', id: string, isFinished?: boolean | null, imageUrl?: string | null, scores?: Array<{ __typename?: 'ScoreSuggestion', position?: number | null, name?: string | null, score?: number | null, isHuman?: boolean | null } | null> | null } | null };

export type ScoresQueryVariables = Exact<{ [key: string]: never; }>;


export type ScoresQuery = { __typename?: 'Query', scores?: Array<{ __typename?: 'Score', id: string, position?: number | null, name?: string | null, score?: number | null, isHuman?: boolean | null, player?: string | null } | null> | null };

export type CreatedScoreSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type CreatedScoreSubscription = { __typename?: 'Subscription', createdScore?: { __typename?: 'Score', id: string, position?: number | null, name?: string | null, score?: number | null, isHuman?: boolean | null, player?: string | null } | null };


export const CreateJobDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"createJob"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateJobInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createJob"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"uploadUrl"}},{"kind":"Field","name":{"kind":"Name","value":"imageUrl"}}]}}]}}]} as unknown as DocumentNode<CreateJobMutation, CreateJobMutationVariables>;
export const CreateScoreDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"createScore"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateScoreInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createScore"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"position"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"score"}},{"kind":"Field","name":{"kind":"Name","value":"isHuman"}},{"kind":"Field","name":{"kind":"Name","value":"player"}}]}}]}}]} as unknown as DocumentNode<CreateScoreMutation, CreateScoreMutationVariables>;
export const UpdatedJobDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"updatedJob"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updatedJob"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFinished"}},{"kind":"Field","name":{"kind":"Name","value":"imageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"scores"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"position"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"score"}},{"kind":"Field","name":{"kind":"Name","value":"isHuman"}}]}}]}}]}}]} as unknown as DocumentNode<UpdatedJobSubscription, UpdatedJobSubscriptionVariables>;
export const ScoresDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"scores"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"scores"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"position"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"score"}},{"kind":"Field","name":{"kind":"Name","value":"isHuman"}},{"kind":"Field","name":{"kind":"Name","value":"player"}}]}}]}}]} as unknown as DocumentNode<ScoresQuery, ScoresQueryVariables>;
export const CreatedScoreDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"createdScore"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createdScore"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"position"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"score"}},{"kind":"Field","name":{"kind":"Name","value":"isHuman"}},{"kind":"Field","name":{"kind":"Name","value":"player"}}]}}]}}]} as unknown as DocumentNode<CreatedScoreSubscription, CreatedScoreSubscriptionVariables>;