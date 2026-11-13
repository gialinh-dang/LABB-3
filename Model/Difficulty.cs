using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LABB_3.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]   // saves "Easy" instead of 0 in the JSON file
public enum Difficulty
{
    Easy,
    Medium,
    Hard,
}