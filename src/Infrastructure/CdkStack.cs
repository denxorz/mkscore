using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.IAM;

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

        var GetUploadUrlLambda = new Function(
            this,
            "MkScoreGetUploadUrlLambda",
            new FunctionProps
            {
                Runtime = Runtime.NODEJS_LATEST,
                Handler = "index.handler",
                Environment = new Dictionary<string, string>() { { "IncomingImagesBucket", IncomingImagesBucket.BucketName } },
                Code = Code.FromInline(@"
                    const { getSignedUrl } = require('@aws-sdk/s3-request-presigner');
                    const { S3Client, PutObjectCommand } = require('@aws-sdk/client-s3');
                    const crypto = require('crypto');

                    exports.handler = async function(event) {
                        const bucket = process.env.IncomingImagesBucket;
                        const s3Client = new S3Client({});
                        const uuid = crypto.randomUUID();
                        const command = new PutObjectCommand({Bucket:bucket, Key:`uploads/${uuid}` });
                        const url = await getSignedUrl(s3Client, command, { expiresIn: 600 });

                        return {
                            statusCode: 200,
                            headers: {
                              'Content-Type': 'application/json',
                              'Access-Control-Allow-Origin': '*',
                              'Access-Control-Allow-Methods': 'OPTIONS,POST,GET,PUT',
                              'Access-Control-Allow-Headers': '*'
                            },
                            body: JSON.stringify({
                                preSignedUrl: url
	                        })
                        };
                    };
                    "),
            }
        );
        IncomingImagesBucket.GrantWrite(GetUploadUrlLambda);

        var api = new LambdaRestApi(
            this,
            "MkScoreApi",
            new LambdaRestApiProps
            {
                Handler = GetUploadUrlLambda,
                DefaultCorsPreflightOptions = new CorsOptions
                {
                    AllowOrigins = ["*"],
                    AllowMethods = ["*"],
                    AllowHeaders = ["*"],
                    AllowCredentials = true,
                },
            }
        );

        var getUploadUrlApi = api.Root.AddResource("getUploadUrl");
        getUploadUrlApi.AddMethod("GET");

        var DetectScoreLambda = new Function(
            this,
            "MkScoreDetectScoreLambda",
            new FunctionProps
            {
                Runtime = Runtime.NODEJS_LATEST,
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
                Handler = "ExtractPlayersLambda::ExtractPlayersLambda.Function::FunctionHandler",
                Timeout = Duration.Minutes(1),
                Environment = new Dictionary<string, string>() { { "IncomingImagesBucket", IncomingImagesBucket.BucketName } },
                Code = Code.FromCustomCommand(
                    "src/ExtractPlayersLambda/function.zip",
                    ["dotnet lambda package -pl src/ExtractPlayersLambda -o src/ExtractPlayersLambda/function.zip"],
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

        var graphQlApi = new Api(this);
        graphQlApi.GrantQuery(ExtractPlayersLambda);
        graphQlApi.GrantMutation(ExtractPlayersLambda);
    }
}
