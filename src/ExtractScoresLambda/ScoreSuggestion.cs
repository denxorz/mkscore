﻿namespace ExtractScoresLambda;

internal record ScoreSuggestion(string Id, bool IsFinished, string ImageUrl, List<PlayerStats> Scores);
