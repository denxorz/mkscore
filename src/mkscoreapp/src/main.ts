import { createApp } from 'vue'
import { registerPlugins } from '@/plugins'
import { DefaultApolloClient } from '@vue/apollo-composable'
import { ApolloClient, ApolloLink, HttpLink, InMemoryCache } from '@apollo/client/core'
import { AUTH_TYPE, createAuthLink, type AuthOptions } from 'aws-appsync-auth-link';
import { createSubscriptionHandshakeLink } from 'aws-appsync-subscription-link';
import { createVInlineFields } from '@wdns/vuetify-inline-fields';
import { createAuth0 } from '@auth0/auth0-vue';

import App from './App.vue'

import buffer from "buffer"
window.Buffer = buffer.Buffer


const auth0Client = createAuth0({
  domain: "dev-vgge3ka7mx15h1rv.eu.auth0.com",
  clientId: "RjCQ7AwQDQyYxX3f5ECIJfzCvyYKzZfy",
  authorizationParams: {
    redirect_uri: window.location.origin
  }
});

const url = import.meta.env.VITE_GraphQLAPI;
const region = 'eu-west-1';
const auth: AuthOptions = {
  type: AUTH_TYPE.OPENID_CONNECT,
  //apiKey: import.meta.env.VITE_GraphQLAPIKey,
  jwtToken: async () => {
    const token = await auth0Client.getAccessTokenSilently();
    console.log({ token })
    return token;
  }, // Required when you use Cognito UserPools OR OpenID Connect. token object is obtained previously
  // credentials: async () => credentials, // Required when you use IAM-based auth.
};

const httpLink = new HttpLink({ uri: url });

const link = ApolloLink.from([
  createAuthLink({ url, region, auth }),
  createSubscriptionHandshakeLink({ url: import.meta.env.VITE_GraphQLWsAPI, region, auth }, httpLink),
]);

const apolloClient = new ApolloClient({
  link,
  cache: new InMemoryCache(),
});

const VInlineFields = createVInlineFields({
  // See Shared Props section for available options
});

const app = createApp(App)
app.provide(DefaultApolloClient, apolloClient)

app.use(VInlineFields);
app.use(auth0Client);

registerPlugins(app)

app.mount('#app')
