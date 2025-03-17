using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using ExtractScoresLambda.Textract;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExtractScoresLambda;

public class Function
{
    private readonly IAmazonS3 s3Client;
    private readonly IGraphQLWebSocketClient graphqlClient;
    private readonly string apiKey = "";
    private readonly string jobImagesBucketName = "";
    private readonly string extractedImagesBucketName = "";

    public Function()
    {
        s3Client = new AmazonS3Client();

        string url = Environment.GetEnvironmentVariable("GraphQLAPIURL") ?? "";
        apiKey = Environment.GetEnvironmentVariable("GraphQLAPIKey") ?? "";

        jobImagesBucketName = Environment.GetEnvironmentVariable("JobImagesBucket") ?? "";
        extractedImagesBucketName = Environment.GetEnvironmentVariable("ExtractedImagesBucket") ?? "";

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
            var objectKey = record.Object.Key.Replace(".json", "");

            try
            {
                var response = await s3Client.GetObjectStreamAsync(bucketName, record.Object.Key, null);
                var extract = await System.Text.Json.JsonSerializer.DeserializeAsync<ExtractedText>(response);

                var responseImage = await s3Client.GetObjectStreamAsync(jobImagesBucketName, objectKey, null);

                using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(responseImage);

                if (extract is not null)
                {
                    using var tableImageStream = ExtractCroppedImage(image, extract.Page);
                    await s3Client.PutObjectAsync(new() { BucketName = extractedImagesBucketName, Key = $"{objectKey}.png", InputStream = tableImageStream });
                    var imageUrl = s3Client.GetPreSignedURLAsync(new() { BucketName = extractedImagesBucketName, Key = $"{objectKey}.png", Expires = DateTime.Now.AddMinutes(15), Verb = HttpVerb.GET });

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
                            int humanLevel = HumanLevel(MapCoordinatesToImage(lines[i + 1], image), image);

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

                    var updateJobRequest = new GraphQLHttpRequestWithAuthSupport
                    {
                        ApiKey = apiKey,
                        Query = """
                            mutation updateJob($input: UpdateJobInput!) {
                                updateJob(input: $input) {
                                    id
                                    isFinished
                                    imageUrl
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
                            input = new ScoreSuggestion(objectKey, true, await imageUrl, playerStats)
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

    private static Rectangle MapCoordinatesToImage(Block line, Image<Rgba32> image)
    {
        var top = Convert.ToInt32(image.Height * line.Geometry.BoundingBox.Top);
        var height = Convert.ToInt32(image.Height * line.Geometry.BoundingBox.Height);
        var left = Convert.ToInt32(image.Width * line.Geometry.BoundingBox.Left);
        var width = Convert.ToInt32(image.Width * line.Geometry.BoundingBox.Width);
        return new(left, top, width, height);
    }

    private static int HumanLevel(Rectangle box, Image<Rgba32> image)
    {
        var totalCellColor = 0;

        image.ProcessPixelRows(accessor =>
        {

            for (int y = 0; y < box.Height; y++)
            {
                Span<Rgba32> pixelRow = accessor.GetRowSpan(box.Y + y);

                for (int x = 0; x < box.Width; x++)
                {
                    ref Rgba32 pixel = ref pixelRow[box.X + x];
                    totalCellColor += pixel.R;
                    totalCellColor += pixel.G;
                    totalCellColor += pixel.B;
                }
            }
        });

        return totalCellColor;
    }

    private static MemoryStream ExtractCroppedImage(Image<Rgba32> image, Block block)
    {
        var blockCoordinates = MapCoordinatesToImage(block, image);

        var outStream = new MemoryStream();
        var clone = image.Clone(i => i.Crop(blockCoordinates));
        clone.Save(outStream, new PngEncoder());
        outStream.Seek(0, SeekOrigin.Begin);

        return outStream;
    }
}
