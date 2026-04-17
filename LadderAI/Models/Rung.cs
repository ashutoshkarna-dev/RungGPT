using System.Text.Json.Serialization;

namespace LadderAI;

public class Rung
{

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("branches")]
    public List<Branch> Branches { get; set; } = new();
}
