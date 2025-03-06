using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using ExtractPlayersLambda.Textract;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExtractPlayersLambda;

public class Function
{
    private readonly IAmazonS3 s3Client;
    private readonly IGraphQLWebSocketClient graphqlClient;
    private readonly string apiKey = "";

    public Function()
    {
        s3Client = new AmazonS3Client();

        string url = Environment.GetEnvironmentVariable("GraphQLAPIURL") ?? "";
        apiKey = Environment.GetEnvironmentVariable("GraphQLAPIKey") ?? "";

        graphqlClient = new GraphQLHttpClient(url, new SystemTextJsonSerializer());
    }

    public Function(IAmazonS3 s3Client, IGraphQLWebSocketClient graphqlClient)
    {
        this.s3Client = s3Client;
        this.graphqlClient = graphqlClient;
    }

    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var eventRecords = evnt.Records ?? [];
        foreach (var record in eventRecords.Select(r => r.S3).Where(r => r is not null))
        {
            var bucketName = record.Bucket.Name;
            var objectKey = record.Object.Key.Replace("extract/", "").Replace(".json", "");

            try
            {
                var response = await s3Client.GetObjectStreamAsync(bucketName, record.Object.Key, null);
                var extract = await System.Text.Json.JsonSerializer.DeserializeAsync<ExtractedText>(response);

                var responseImage = await s3Client.GetObjectStreamAsync(bucketName, $"uploads/{objectKey}", null);

                using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(responseImage);

                if (extract is not null)
                {
                    var lines = extract.Page.Children(extract);
                    var playerStats = new List<PlayerStats>();

                    var firstLine = lines.First(l => l.Text == "1"); // TODO : handle if not found
                    var positionLineIndex = lines.IndexOf(firstLine);

                    for (int i = positionLineIndex; i < 12 * 3; i += 3)
                    {
                        try
                        {
                            int.TryParse(lines[i].Text, out var position);
                            string name = lines[i + 1].Text;
                            int.TryParse(lines[i + 2].Text, out var score);
                            int humanLevel = HumanLevel(lines[i + 1], image);

                            playerStats.Add(new PlayerStats(position, name, score, humanLevel));
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // TODO : assumes everything is detected
                        }
                    }

                    // TODO : assumes always 4 players
                    playerStats.OrderByDescending(p => p.IsHuman).Take(4).ToList().ForEach(p => p.IsHuman = true);

                    foreach (var player in playerStats)
                    {
                        context.Logger.LogInformation($"{player.Position}. {player.Name} {player.Score} {(player.IsHuman ? "Player" : "")}");
                    }

                    using var outputStream = new MemoryStream();
                    await System.Text.Json.JsonSerializer.SerializeAsync(outputStream, playerStats);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    var putRequest = new PutObjectRequest { BucketName = bucketName, Key = $"stats/{objectKey}.json", InputStream = outputStream };
                    await s3Client.PutObjectAsync(putRequest);



                    var updateJobRequest = new GraphQLHttpRequestWithAuthSupport
                    {
                        ApiKey = apiKey,
                        Query = """
                            mutation updateJob($input: UpdateJobInput!) {
                                updateJob(input: $input) {
                                    id
                                    isFinished
                                    scores {
                                        position
                                        name
                                        score
                                        isHuman
                                    }
                                }
                            }
                            """,
                        OperationName = "updateJob",
                        Variables = new
                        {
                            input = new ScoreSuggestion(objectKey, true, playerStats)
                        }
                    };
                    var res = await graphqlClient.SendMutationAsync<ScoreSuggestion>(updateJobRequest);
                    context.Logger.LogInformation($"SendMutationAsync:{string.Join("/", res.Errors?.Select(e => e.Message) ?? [])}");
                }
            }
            catch (Exception e)
            {
                context.Logger.LogError($"Error getting object {objectKey} from bucket {bucketName}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogError(e.Message);
                context.Logger.LogError(e.StackTrace);
                throw;
            }
        }
    }

    private static int HumanLevel(Block line, Image<Rgba32> image)
    {
        var top = Convert.ToInt32(image.Height * line.Geometry.BoundingBox.Top);
        var height = Convert.ToInt32(image.Height * line.Geometry.BoundingBox.Height);
        var left = Convert.ToInt32(image.Width * line.Geometry.BoundingBox.Left);
        var width = Convert.ToInt32(image.Width * line.Geometry.BoundingBox.Width);

        var totalCellColor = 0;

        image.ProcessPixelRows(accessor =>
        {

            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> pixelRow = accessor.GetRowSpan(top + y);

                for (int x = 0; x < width; x++)
                {
                    ref Rgba32 pixel = ref pixelRow[left + x];
                    totalCellColor += pixel.R;
                    totalCellColor += pixel.G;
                    totalCellColor += pixel.B;
                }
            }
        });

        return totalCellColor;
    }
}
