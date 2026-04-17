using System.IO;
using System.Text;
using System.Text.Json;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace LadderAI.Services;

public class OllamaLadderService
{
    private readonly OllamaApiClient _client;
    private readonly string _systemPrompt;
    private readonly LadderValidator _validator = new();
    private readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OllamaLadderService(string systemPromptPath, string model="gemma4:e4b")
    {
        _client = new OllamaApiClient(new Uri("http://localhost:11434"))
        {
            SelectedModel = model
        };
        _systemPrompt = File.ReadAllText(systemPromptPath);
    }

    public async Task<GenerationResult> GenerateAsync(string userRequest, int maxRetries = 2)
    {
        string lastJson = "";
        string? repairNote = null;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            lastJson = await CallModelAsync(userRequest, repairNote);

            LadderProgram? program = null;
            string? parseError = null;
            try
            {
                program = JsonSerializer.Deserialize<LadderProgram>(lastJson, _jsonOpts);

            }
            catch (JsonException ex)
            {
                parseError = ex.Message;
            }

            if (program is null)
            {
                repairNote = BuildRepairPrompt(lastJson,
                    $"JSON parse failed: {parseError}. Return corrected full JSON.");
                continue;
            }

            var issues = _validator.Validate(program);
            var errors = issues.Where(i => i.Level == Severity.Error).ToList();
            if (errors.Count == 0)
            {
                return new GenerationResult(program, issues, attempt, lastJson);
            }

            repairNote = BuildRepairPrompt(lastJson,
                "Validation errors to fix:\n" +
                string.Join("\n", errors.Select(e => $"- {e.Path}: {e.Message}")) +
                "\nReturn corrected full JSON.");
        }
        throw new InvalidOperationException(
            $"Model did not produce valid ladder after {maxRetries + 1} attempts. Last output:\n{lastJson}");
    }

    private async Task<string> CallModelAsync(string userRequest, string? repairNote)
    {
        var messages = new List<Message>
        {
            new() {Role = ChatRole.System, Content = _systemPrompt}
        };
        if (repairNote != null)
        {
            messages.Add(new Message { Role = ChatRole.User, Content = repairNote });
        }
        messages.Add(new Message { Role = ChatRole.User, Content = userRequest });

        var request = new ChatRequest
        {
            Model = _client.SelectedModel,
            Messages = messages,
            Format = "json",
            Stream = false,
            Options = new RequestOptions
            {
                Temperature = 0.2f,
                TopP = 0.9f
            }
        };

        var sb = new StringBuilder();
        await foreach (var token in _client.ChatAsync(request))
        {
            if (token?.Message?.Content is { } c) sb.Append(c);
        }
        return sb.ToString().Trim();
    }

    private static string BuildRepairPrompt(string previousJson, string errorDescription) =>
        $"Previous output:\n{previousJson}\n\n{errorDescription}";
}

public record GenerationResult(
    LadderProgram Program,
    List<ValidationIssue> Issues,
    int Attempts,
    string RawJson);
