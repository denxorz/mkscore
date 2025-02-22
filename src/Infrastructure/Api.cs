using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;

namespace Infrastructure;

internal class Api
{
    private readonly GraphqlApi api;

    public Api(Stack stack)
    {
        api = new GraphqlApi(
            stack,
            "MKScoreApi",
            new GraphqlApiProps
            {
                Name = "MKScoreApi",
                Definition = Definition.FromFile("./src/Infrastructure/apischema.graphql"),
                AuthorizationConfig = new AuthorizationConfig
                {
                    DefaultAuthorization = new AuthorizationMode
                    {
                        AuthorizationType = AuthorizationType.API_KEY,
                        ApiKeyConfig = new ApiKeyConfig
                        {
                            Name = "CDK / Lambda 2025",
                            Description = "CDK / Lambda",
                            Expires = Expiration.After(Duration.Days(365)),
                        }
                    }
                },
            });

        var jobsTable = new Table(
            stack,
            "MKScoreJobsTable",
            new TableProps
            {
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING,
                },
            });

        var jobsDataSource = api.AddDynamoDbDataSource("MKScoreJobsDataSource", jobsTable);

        jobsDataSource.CreateResolver(
            "MKScoreJobsGetResolver",
            new ResolverProps
            {
                TypeName = "Query",
                FieldName = "job",
                Api = api,
                DataSource = jobsDataSource,
                RequestMappingTemplate = MappingTemplate.DynamoDbQuery(KeyCondition.Eq("id", "id")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

        jobsDataSource.CreateResolver(
            "MKScoreJobsAddResolver",
            new ResolverProps
            {
                TypeName = "Mutation",
                FieldName = "createJob",
                Api = api,
                DataSource = jobsDataSource,
                RequestMappingTemplate = MappingTemplate.DynamoDbPutItem(PrimaryKey.Partition("id").Auto(), Values.Projecting("input")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

        jobsDataSource.CreateResolver(
            "MKScoreJobsUpdateResolver",
            new ResolverProps
            {
                TypeName = "Mutation",
                FieldName = "updateJob",
                Api = api,
                DataSource = jobsDataSource,
                RequestMappingTemplate = MappingTemplate.DynamoDbPutItem(PrimaryKey.Partition("id").Is("input.id"), Values.Projecting("input")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

        _ = new CfnOutput(stack, "GraphQLAPIURL", new CfnOutputProps { Value = api.GraphqlUrl });
        _ = new CfnOutput(stack, "GraphQLAPIKey", new CfnOutputProps { Value = api.ApiKey ?? "" });
    }

    public void GrantQuery(Function function, params string[] fields)
    {
        api.GrantQuery(function, fields);
        function.AddEnvironment("GraphQLAPIURL", api.GraphqlUrl);
        function.AddEnvironment("GraphQLAPIKey", api.ApiKey);
    }

    public void GrantMutation(Function function, params string[] fields)
    {
        api.GrantMutation(function, fields);
        function.AddEnvironment("GraphQLAPIURL", api.GraphqlUrl);
        function.AddEnvironment("GraphQLAPIKey", api.ApiKey);
    }
}
