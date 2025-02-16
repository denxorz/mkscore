namespace ExtractPlayersLambda;

internal record PlayerStats(int Position, string Name, int Score, int HumanLevel)
{
    public bool IsHuman { get; set; }
}
