namespace ExtractScoresLambda.Textract;

public class Metadata
{
    public int httpStatusCode { get; set; }
    public string requestId { get; set; }
    public int attempts { get; set; }
    public int totalRetryDelay { get; set; }
}
