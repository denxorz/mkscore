namespace ExtractPlayersLambda.Textract;

public class ExtractedText
{
    public Metadata metadata { get; set; }
    public string AnalyzeDocumentModelVersion { get; set; }
    public Block[] Blocks { get; set; }
    public Documentmetadata DocumentMetadata { get; set; }

    public Block Page => Blocks.First(b => b.BlockType == "PAGE");

    private Dictionary<string, Block>? blocksById;
    public Dictionary<string, Block> BlocksById => blocksById ??= Blocks.ToDictionary(b => b.Id, b => b);
    public Block? TryGetBlock(string id) => BlocksById.TryGetValue(id, out var res) ? res : null;


    private Dictionary<string, Block>? linesById;
    public Dictionary<string, Block> LinesById => linesById ??= BlocksById.Where(b => b.Value.BlockType == "LINE").ToDictionary(b => b.Key, b => b.Value);
    public Block? TryGetLine(string id) => LinesById.TryGetValue(id, out var res) ? res : null;
}
