using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using ExtractPlayersLambda.Textract;
using GraphQL.Client.Abstractions;
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
    private readonly string apiKey;

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

    class PlayerStats
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public bool IsHuman { get; set; }
        public int HumanLevel { get; set; }
    }

    record ScoreEntry(string Id, bool IsFinished, List<PlayerStats> Scores);

    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();
        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;
            if (s3Event == null)
            {
                continue;
            }

            try
            {
                var response = await s3Client.GetObjectStreamAsync(s3Event.Bucket.Name, s3Event.Object.Key, null);
                var extract = System.Text.Json.JsonSerializer.Deserialize<ExtractedText>(response);

                var responseImage = await s3Client.GetObjectStreamAsync(s3Event.Bucket.Name, s3Event.Object.Key.Replace("extract", "uploads").Replace(".json", ""), null);

                using Image<Rgba32> image = Image.Load<Rgba32>(responseImage);

                if (extract is not null)
                {
                    var page = extract.Page;
                    var lines = extract.Page.Children(extract, context);
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

                            playerStats.Add(new PlayerStats { Position = position, Name = name, Score = score, HumanLevel = humanLevel });
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
                    var putRequest = new PutObjectRequest { BucketName = s3Event.Bucket.Name, Key = s3Event.Object.Key.Replace("extract", "stats"), InputStream = outputStream };
                    await s3Client.PutObjectAsync(putRequest);



                    var updateJobRequest = new GraphQLHttpRequestWithAuthSupport
                    {
                        ApiKey = apiKey,
                        Query = """
                            mutation UpdateJob($input: UpdateJobInput) {
                                updateJob($input: $input) {
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
                            input = new ScoreEntry(s3Event.Object.Key.Replace("extract", ""), true, playerStats)
                        }
                    };
                    await graphqlClient.SendMutationAsync<ScoreEntry>(updateJobRequest);
                }
            }
            catch (Exception e)
            {
                context.Logger.LogError($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
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