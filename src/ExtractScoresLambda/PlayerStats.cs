﻿using System.Text.Json.Serialization;

namespace ExtractScoresLambda;

internal record PlayerStats(
    int Position, 
    string Name, 
    int Score, 
    [property: JsonIgnore] int HumanLevel)
{
    public bool IsHuman { get; set; }
}
