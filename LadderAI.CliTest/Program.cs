using LadderAI.Services;

var prompts = new[] {
    "Motor start/stop. Start=X0, Stop=X1 NC wired, E-stop=X2 NC wired, Motor=Y10.",
    "Turn on Y11 five seconds after X0 goes high.",
    "Count parts on X3. After 10 parts, set M100. Reset with X4.",
    "Conveyor Y20: start on X0, stop on X1 NC, stop if photoeye X5 blocked for 2 seconds.",
    "Two-hand anti-tiedown: both X0 and X1 must press within 0.5s to energize press Y10.",
    "Tank fill: pump Y10 runs when level low X0 is on, stops when level high X1 is on.",
    "Blink Y12 every 1 second while X0 is on.",
    "Traffic light: green Y10 for 10s, yellow Y11 for 3s, red Y12 for 12s, repeat.",
    "Reverse motor: forward on X0, reverse on X1, stop on X2, with interlock.",
    "Alarm Y15 if temperature D100 exceeds 85."
};

var service = new OllamaLadderService(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "LadderAI", "Prompts", "system.md"));

int pass = 0, fail = 0;
foreach (var p in prompts)
{
    Console.WriteLine($"\n=== {p}");
    try
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await service.GenerateAsync(p, maxRetries: 2);
        sw.Stop();

        var warnings = result.Issues.Count(i => i.Level == Severity.Warning);
        Console.WriteLine($"  OK  attempts={result.Attempts + 1}  rungs={result.Program.Rungs.Count}  " +
                          $"devices={result.Program.Devices.Count}  warnings={warnings}  time={sw.Elapsed.TotalSeconds:F1}s");
        pass++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  FAIL  {ex.Message.Split('\n')[0]}");
        fail++;
    }
}

Console.WriteLine($"\nPass: {pass}/{pass + fail}");