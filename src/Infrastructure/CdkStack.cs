using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.DynamoDB;

namespace Infrastructure;

public class CdkStack : Stack
{
    internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var IncomingImagesBucket = new Bucket(
            this,
            "MkScoreIncomingImagesBucket",
            new BucketProps
            {
                BucketName = "mkscore-incoming-images-bucket-1",
                WebsiteIndexDocument = "index.html",
                Cors = [
                    new CorsRule
                        {
                            AllowedHeaders= ["*"],
                            AllowedMethods= [HttpMethods.PUT, HttpMethods.GET, HttpMethods.POST, HttpMethods.HEAD],
                            AllowedOrigins= ["*"],
                            ExposedHeaders= [],
                        }
                ]
            }
        );

        var jobsTable = new Table(
            this,
            "MKScoreJobsTable",
            new TableProps
            {
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING,
                },
            });

        var createJobLambda = new Function(
            this,
            "MkScoreCreateJobLambda",
            new FunctionProps
            {
                Runtime = Runtime.NODEJS_LATEST,
                Architecture = Architecture.ARM_64,
                Handler = "index.handler",
                Environment = new Dictionary<string, string>() { { "IncomingImagesBucket", IncomingImagesBucket.BucketName } },
                Code = Code.FromInline(@"
                    const { getSignedUrl } = require('@aws-sdk/s3-request-presigner');
                    const { S3Client, PutObjectCommand } = require('@aws-sdk/client-s3');
                    const crypto = require('crypto');

                    exports.handler = async function(event) {
                        const bucket = process.env.IncomingImagesBucket;
                        const s3Client = new S3Client({});
                        const id = crypto.randomUUID();

                        const putObjectCommand = new PutObjectCommand({Bucket:bucket, Key:`uploads/${id}` });
                        const uploadUrl = await getSignedUrl(s3Client, putObjectCommand, { expiresIn: 600 });

                        const newJob = {
                            id,
                            name: event.arguments.input.name,
                            isFinished: false,
                            uploadUrl,
                            scores: [],
                        };

                        const { DynamoDBClient, PutItemCommand  } = require('@aws-sdk/client-dynamodb');
                        const { marshall } = require('@aws-sdk/util-dynamodb');
                        const dynamoDbInput = marshall(newJob);
                        const client = new DynamoDBClient({});
                        const jobsTable = process.env.JobsTable;
                        const putItemCommand = new PutItemCommand({TableName:jobsTable, Item:dynamoDbInput});
                        await client.send(putItemCommand);

                        return newJob;
                    };
                    "),
            }
        );
        IncomingImagesBucket.GrantWrite(createJobLambda);

        jobsTable.GrantWriteData(createJobLambda);
        createJobLambda.AddEnvironment("JobsTable", jobsTable.TableName);

        var DetectScoreLambda = new Function(
            this,
            "MkScoreDetectScoreLambda",
            new FunctionProps
            {
                Runtime = Runtime.NODEJS_LATEST,
                Architecture = Architecture.ARM_64,
                Handler = "index.handler",
                Timeout = Duration.Minutes(1),
                Environment = new Dictionary<string, string>() { { "IncomingImagesBucket", IncomingImagesBucket.BucketName } },
                Code = Code.FromInline(@"
                    const { TextractClient, AnalyzeDocumentCommand } = require('@aws-sdk/client-textract');
                    const { S3Client, PutObjectCommand } = require('@aws-sdk/client-s3');

                    exports.handler = async function(event) {
                        const textractClient = new TextractClient({});
                        const analyseCommand = new AnalyzeDocumentCommand({
                          Document:{
                            S3Object: { 
                              Bucket:event.Records[0].s3.bucket.name, 
                              Name:event.Records[0].s3.object.key 
                            }
                          }, 
                          FeatureTypes:['TABLES']
                        });
                        const response = await textractClient.send(analyseCommand);

                        const s3Client = new S3Client({});
                        const putCommand = new PutObjectCommand({
                            Bucket:event.Records[0].s3.bucket.name, 
                            Key:`extract/${event.Records[0].s3.object.key.replace('uploads/', '')}.json`,
                            Body: JSON.stringify(response),
                        });
                        await s3Client.send(putCommand);
                    };
                    "),
            }
        );
        IncomingImagesBucket.GrantReadWrite(DetectScoreLambda);

        DetectScoreLambda.AddEventSource(
            new S3EventSource(
                IncomingImagesBucket,
                new S3EventSourceProps
                {
                    Events = [EventType.OBJECT_CREATED_PUT],
                    Filters = [new NotificationKeyFilter { Prefix = "uploads/" }],
                })
            );

        DetectScoreLambda.AddToRolePolicy(
            new PolicyStatement(
                new PolicyStatementProps
                {
                    Effect = Effect.ALLOW,
                    Actions = ["textract:AnalyzeDocument"],
                    Resources = ["*"]
                }));

        var ExtractPlayersLambda = new Function(
            this,
            "MkScoreExtractPlayersLambda",
            new FunctionProps
            {
                Runtime = Runtime.DOTNET_8,
                Architecture = Architecture.ARM_64,
                Handler = "ExtractPlayersLambda::ExtractPlayersLambda.Function::FunctionHandler",
                Timeout = Duration.Minutes(1),
                MemorySize = 2048,
                Environment = new Dictionary<string, string>() { { "IncomingImagesBucket", IncomingImagesBucket.BucketName } },
                Code = Code.FromCustomCommand(
                    "src/ExtractPlayersLambda/bin/function.zip",
                    ["dotnet lambda package -pl src/ExtractPlayersLambda -o src/ExtractPlayersLambda/bin/function.zip"],
                    new CustomCommandOptions
                    {
                        CommandOptions = new Dictionary<string, object> { { "shell", true } }
                    })
            }
        );
        IncomingImagesBucket.GrantReadWrite(ExtractPlayersLambda);

        ExtractPlayersLambda.AddEventSource(
           new S3EventSource(
               IncomingImagesBucket,
               new S3EventSourceProps
               {
                   Events = [EventType.OBJECT_CREATED_PUT],
                   Filters = [new NotificationKeyFilter { Prefix = "extract/" }],
               })
        );

        var graphQlApi = new ApiStack(this, "MkScoreApiStack", new()
        {
            JobsTable = jobsTable,
            CreateJobLambda = createJobLambda
        });
        graphQlApi.GrantQuery(ExtractPlayersLambda);
        graphQlApi.GrantMutation(ExtractPlayersLambda);

        _ = new VueAppStack(this, "MkScoreVueAppStack", new());
    }
}
