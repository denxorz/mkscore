namespace ExtractPlayersLambda;

internal record ScoreEntry(string Id, bool IsFinished, List<PlayerStats> Scores);
