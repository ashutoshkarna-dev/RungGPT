using System.Text.Json.Serialization;

namespace LadderAI;

public class LadderProgram
{
    [JsonPropertyName("programName")]
    public string ProgramName { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("devices")]
    public List<Device> Devices { get; set; } = new();

    [JsonPropertyName("rungs")]
    public List<Rung> Rungs { get; set; } = new();


}
