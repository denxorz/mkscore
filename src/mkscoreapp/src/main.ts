import { createApp } from 'vue'
import { registerPlugins } from '@/plugins'
import { DefaultApolloClient } from '@vue/apollo-composable'
import { ApolloClient, ApolloLink, concat, createHttpLink, InMemoryCache } from '@apollo/client/core'

import App from './App.vue'

const httpLink = createHttpLink({
    uri: 'https://pgagcjxksvdqbkkfwjdszvag7u.appsync-api.eu-central-1.amazonaws.com/graphql',
})

const authMiddleware = new ApolloLink((operation, forward) => {
    operation.setContext({
        headers: {
            "x-api-key": import.meta.env.VITE_GraphQLAPIKey,
        },
    });
    return forward(operation);
});

export const apolloClient = new ApolloClient({
    link: concat(authMiddleware, httpLink),
    cache: new InMemoryCache(),
});

const app = createApp(App)
app.provide(DefaultApolloClient, apolloClient)
registerPlugins(app)

app.mount('#app')
