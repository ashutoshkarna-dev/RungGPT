using System.Text.Json.Serialization;
using System.Windows.Input;

namespace LadderAI;

public class Device
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("type")]

    public DeviceType Type {get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("preset")]
    public int? Preset { get; set; }

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceType
{
    Input,
    Output,
    Internal,
    Timer,
    Counter,
    DataRegister
}

