using System.Text.Json.Serialization;

namespace LadderAI;

public class Branch
{
    [JsonPropertyName("elements")]
    public List<Element>? Elements { get; set; }

    [JsonPropertyName("parallel")]
    public List<Branch>? Parallel {  get; set; }

}
