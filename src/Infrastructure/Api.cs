using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;

namespace Cdk;

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
                Definition = Definition.FromFile("./src/Cdk/apischema.graphql"),
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
                RequestMappingTemplate = MappingTemplate.DynamoDbPutItem(PrimaryKey.Partition("id").Auto(), Values.Projecting("input")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

        new CfnOutput(stack, "GraphQLAPIURL", new CfnOutputProps { Value = api.GraphqlUrl });
        new CfnOutput(stack, "GraphQLAPIKey", new CfnOutputProps { Value = api.ApiKey ?? "" });
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
