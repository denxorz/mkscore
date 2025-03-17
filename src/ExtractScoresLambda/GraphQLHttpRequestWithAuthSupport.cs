using System.Net.Http.Headers;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;

namespace ExtractScoresLambda;

public class GraphQLHttpRequestWithAuthSupport : GraphQLHttpRequest
{
    public string? ApiKey { get; set; }

    public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
    {
        var r = base.ToHttpRequestMessage(options, serializer);
        r.Headers.Add("x-api-key", ApiKey);
        return r;
    }
}