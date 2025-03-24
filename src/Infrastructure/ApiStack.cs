using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;

namespace Infrastructure;

internal class ApiStack
{
    private readonly GraphqlApi api;

    public ApiStack(Stack stack, string id, ApiStackProps props)
    {
        api = new GraphqlApi(
            stack,
            id + "Api",
            new GraphqlApiProps
            {
                Name = "MkScoreApi",
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

        var jobsDynamoDbDataSource = api.AddDynamoDbDataSource(id + "JobsDynamoDataSource", props.JobsTable);

        jobsDynamoDbDataSource.CreateResolver(
            id + "JobsGetResolver",
            new ResolverProps
            {
                TypeName = "Query",
                FieldName = "job",
                Api = api,
                DataSource = jobsDynamoDbDataSource,
                RequestMappingTemplate = MappingTemplate.DynamoDbQuery(KeyCondition.Eq("id", "id")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

        jobsDynamoDbDataSource.CreateResolver(
            id + "JobsUpdateResolver",
            new ResolverProps
            {
                TypeName = "Mutation",
                FieldName = "updateJob",
                Api = api,
                DataSource = jobsDynamoDbDataSource,
                RequestMappingTemplate = MappingTemplate.DynamoDbPutItem(PrimaryKey.Partition("id").Is("input.id"), Values.Projecting("input")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

        var jobsLambdaDataSource = api.AddLambdaDataSource(id + "JobsLambdaDataSource", props.CreateJobLambda);

        jobsLambdaDataSource.CreateResolver(
            id + "JobsCreateResolver",
            new ResolverProps
            {
                TypeName = "Mutation",
                FieldName = "createJob",
                Api = api
            });


        var scoresTable = new Table(
            stack,
            id + "ScoresTable",
            new TableProps
            {
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING,
                },
            });

        var scoresDynamoDbDataSource = api.AddDynamoDbDataSource(id + "ScoresDynamoDataSource", scoresTable);

        scoresDynamoDbDataSource.CreateResolver(
            id + "ScoresListResolver",
            new ResolverProps
            {
                TypeName = "Query",
                FieldName = "scores",
                Api = api,
                DataSource = scoresDynamoDbDataSource,
                RequestMappingTemplate = MappingTemplate.DynamoDbScanTable(),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultList(),
            });

        scoresDynamoDbDataSource.CreateResolver(
            id + "ScoresCreateResolver",
            new ResolverProps
            {
                TypeName = "Mutation",
                FieldName = "createScore",
                Api = api,
                RequestMappingTemplate = MappingTemplate.DynamoDbPutItem(PrimaryKey.Partition("id").Is("input.id"), Values.Projecting("input")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

        _ = new CfnOutput(stack, "APIURL", new CfnOutputProps { Value = api.GraphqlUrl });
        _ = new CfnOutput(stack, "APIKey", new CfnOutputProps { Value = api.ApiKey ?? "" });
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

    public class ApiStackProps : StackProps
    {
        public Table JobsTable { get; set; }
        public Function CreateJobLambda { get; set; }
    }
}
