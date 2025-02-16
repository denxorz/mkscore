using Amazon.Lambda.Core;

namespace MkScoreExtractPlayersLambda.Textract;

public class Block
{
    public string BlockType { get; set; }
    public Geometry Geometry { get; set; }
    public string Id { get; set; }
    public Relationship[] Relationships { get; set; }
    public float Confidence { get; set; }
    public string Text { get; set; }
    public string TextType { get; set; }
    public string[] EntityTypes { get; set; }
    public int ColumnIndex { get; set; }
    public int ColumnSpan { get; set; }
    public int RowIndex { get; set; }
    public int RowSpan { get; set; }

    public List<Block> Children(ExtractedText page, ILambdaContext context)
    {
        if (Relationships is null) return [];
        return Relationships[0]?.Ids?.Select(page.TryGetBlock).Where(b => b is not null).Select(b => b!).ToList() ?? [];
    }

    public List<Block?> Lines(ExtractedText page, ILambdaContext context)
    {
        if (Relationships is null) return [];
        return Relationships[0]?.Ids?.Select(page.TryGetLine).Where(b => b.BlockType == "").ToList() ?? [];
    }

    public string TextWithChild(ExtractedText page, ILambdaContext context)
    {
        return $"{Text}:[{string.Join(',', Children(page, context).Select(c => c?.TextWithChild(page, context)))}]"; ;
    }
}
