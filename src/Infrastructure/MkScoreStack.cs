using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.CertificateManager;

namespace Infrastructure;

public class MkScoreStack : Stack
{
    internal MkScoreStack(Construct scope, string id, IStackProps props = null)
        : base(scope, id, props)
    {
        var isDev = id.EndsWith("Dev");
        var jobImagesBucket = new Bucket(
            this,
            "JobImagesBucket",
            new BucketProps
            {
                BucketName = $"mkscore{(isDev ? "-dev" : "")}-job-images-bucket",
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

        var extractedTextBucket = new Bucket(
            this,
            "ExtractedTextBucket",
            new BucketProps
            {
                BucketName = $"mkscore{(id.EndsWith("Dev") ? "-dev" : "")}-extracted-text-bucket",
            }
        );

        var extractedImagesBucket = new Bucket(
            this,
            "ExtractedImagesBucket",
            new BucketProps
            {
                BucketName = $"mkscore{(id.EndsWith("Dev") ? "-dev" : "")}-extracted-images-bucket",
            }
        );

        var jobsTable = new Table(
            this,
            "JobsTable",
            new TableProps
            {
                BillingMode = BillingMode.PAY_PER_REQUEST,                
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING,
                },
            });

        var createJobLambda = new Function(
            this,
            "CreateJobLambda",
            new FunctionProps
            {
                Runtime = Runtime.NODEJS_LATEST,
                Architecture = Architecture.ARM_64,
                Handler = "index.handler",
                Code = Code.FromInline(@"
                    const { getSignedUrl } = require('@aws-sdk/s3-request-presigner');
                    const { S3Client, PutObjectCommand } = require('@aws-sdk/client-s3');
                    const crypto = require('crypto');

                    exports.handler = async function(event) {
                        const bucket = process.env.JobImagesBucket;
                        const s3Client = new S3Client({});
                        const id = crypto.randomUUID();

                        const putObjectCommand = new PutObjectCommand({Bucket:bucket, Key:id });
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
        jobImagesBucket.GrantWrite(createJobLambda);
        createJobLambda.AddEnvironment("JobImagesBucket", jobImagesBucket.BucketName);

        jobsTable.GrantWriteData(createJobLambda);
        createJobLambda.AddEnvironment("JobsTable", jobsTable.TableName);

        var extractTextLambda = new Function(
            this,
            "ExtractTextLambda",
            new FunctionProps
            {
                Runtime = Runtime.NODEJS_LATEST,
                Architecture = Architecture.ARM_64,
                Handler = "index.handler",
                Timeout = Duration.Minutes(1),
                Code = Code.FromInline(@"
                    const { TextractClient, AnalyzeDocumentCommand } = require('@aws-sdk/client-textract');
                    const { S3Client, PutObjectCommand } = require('@aws-sdk/client-s3');

                    exports.handler = async function(event) {
                        const textractClient = new TextractClient({});
                        const analyseCommand = new AnalyzeDocumentCommand({
                          Document:{
                            S3Object: { 
                              Bucket: event.Records[0].s3.bucket.name, 
                              Name: event.Records[0].s3.object.key 
                            }
                          }, 
                          FeatureTypes:['TABLES']
                        });
                        const response = await textractClient.send(analyseCommand);

                        const outBucket = process.env.ExtractedTextBucket;
                        const s3Client = new S3Client({});
                        const putCommand = new PutObjectCommand({
                            Bucket: outBucket, 
                            Key:`${event.Records[0].s3.object.key}.json`,
                            Body: JSON.stringify(response),
                        });
                        await s3Client.send(putCommand);
                    };
                    "),
            }
        );
        jobImagesBucket.GrantRead(extractTextLambda);

        extractedTextBucket.GrantWrite(extractTextLambda);
        extractTextLambda.AddEnvironment("ExtractedTextBucket", extractedTextBucket.BucketName);
        extractTextLambda.AddEventSource(new S3EventSource(jobImagesBucket, new S3EventSourceProps { Events = [EventType.OBJECT_CREATED_PUT] }));

        extractTextLambda.AddToRolePolicy(
            new PolicyStatement(
                new PolicyStatementProps
                {
                    Effect = Effect.ALLOW,
                    Actions = ["textract:AnalyzeDocument"],
                    Resources = ["*"]
                }));

        var extractScoresLambda = new Function(
            this,
            "ExtractScoresLambda",
            new FunctionProps
            {
                Runtime = Runtime.DOTNET_8,
                Architecture = Architecture.ARM_64,
                Handler = "ExtractScoresLambda::ExtractScoresLambda.Function::FunctionHandler",
                Timeout = Duration.Minutes(1),
                MemorySize = 2048,
                Code = Code.FromCustomCommand(
                    "src/ExtractScoresLambda/bin/function.zip",
                    ["dotnet lambda package -pl src/ExtractScoresLambda -o src/ExtractScoresLambda/bin/function.zip"],
                    new CustomCommandOptions
                    {
                        CommandOptions = new Dictionary<string, object> { { "shell", true } }
                    })
            }
        );
        extractedTextBucket.GrantRead(extractScoresLambda);

        extractedImagesBucket.GrantReadWrite(extractScoresLambda);
        extractScoresLambda.AddEnvironment("ExtractedImagesBucket", extractedImagesBucket.BucketName);

        jobImagesBucket.GrantRead(extractScoresLambda);
        extractScoresLambda.AddEnvironment("JobImagesBucket", jobImagesBucket.BucketName);

        extractedTextBucket.GrantRead(extractScoresLambda);
        extractScoresLambda.AddEventSource(new S3EventSource(extractedTextBucket, new S3EventSourceProps { Events = [EventType.OBJECT_CREATED_PUT] }));


        var certificateArn = "arn:aws:acm:us-east-1:586794442045:certificate/6658610a-6330-49c5-9c10-3a176f9dcc97";
        var certificate = Certificate.FromCertificateArn(this, id + "Certificate", certificateArn);
        //var certificate = new Certificate(this, id+"Certificate", new CertificateProps { DomainName = "mkscoreapp.geldhof.eu" }); 

        var graphQlApi = new ApiStack(
            this,
            "ApiStack",
            new()
            {
                JobsTable = jobsTable,
                CreateJobLambda = createJobLambda,
                IsDev = isDev,
                Certificate = certificate,
            });
        graphQlApi.GrantQuery(extractScoresLambda);
        graphQlApi.GrantMutation(extractScoresLambda);

        _ = new VueAppStack(
            this,
            "VueAppStack",
            new()
            {
                IsDev = isDev,
                Certificate = certificate,
            });
    }
}
