namespace ExtractPlayersLambda;

internal record ScoreSuggestion(string Id, bool IsFinished, List<PlayerStats> Scores);
