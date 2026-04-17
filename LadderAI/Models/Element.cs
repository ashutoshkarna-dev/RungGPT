using System.Text.Json.Serialization;

namespace LadderAI;

public class Element
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("device")]
    public string Device { get; set; } = "";

    [JsonPropertyName("normallyClosed")]
    public bool NormallyClosed { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("preset")]
    public int? Preset { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("destination")]
    public string? Destination { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("operand1")]
    public string? Operand1 { get; set; }

    [JsonPropertyName("operand2")]
    public string? Operand2 { get; set; }

}
