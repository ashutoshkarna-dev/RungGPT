using System.Text.RegularExpressions;
using System.Xml.Schema;

namespace LadderAI.Services;

public record ValidationIssue(string Path, string Message, Severity Level);
public enum Severity { Error, Warning }
public class LadderValidator
{
    // iQ-R device prefixes we accept
    private static readonly Regex DeviceRx =
        new(@"^(X|Y|M|L|B|F|SB|SM|T|ST|C|D|W|R|ZR|SD)[0-9A-F]+$", RegexOptions.Compiled);

    public List<ValidationIssue> Validate(LadderProgram p)
    {
        var issues = new List<ValidationIssue>();
        if(p is null)
        {
            issues.Add(new("$", "Program is null", Severity.Error));
            return issues;
        }

        ValidateProgramHeader(p, issues);
        var deviceMap = ValidateDevices(p, issues);
        ValidateRungs(p, deviceMap, issues);
        return issues;
    }

    void ValidateProgramHeader(LadderProgram p, List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(p.ProgramName))
            issues.Add(new("$.programName", "Missing program name", Severity.Error));
        else if (!Regex.IsMatch(p.ProgramName, @"^[A-Za-z_][A - Za - z0 - 9_]{ 0,31}$"))
            issues.Add(new("$.programName", "Invalid identifier (<= 32 chars, start with letter/_)", Severity.Error));


    }

    Dictionary<string, Device> ValidateDevices(LadderProgram p, List<ValidationIssue> issues)
    {
        var map = new Dictionary<string, Device>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < p.Devices.Count; i++)
        {
            var d = p.Devices[i];
            var path = $"$.devices[{i}]";

            if (string.IsNullOrWhiteSpace(d.Name))
            {
                issues.Add(new(path, "Empty device name", Severity.Error)); continue;
            }
            if (!DeviceRx.IsMatch(d.Name))
                issues.Add(new(path, $"'{d.Name}' is not a valid iQ-R device", Severity.Error));

            if (!IsPrefixMatch(d.Name, d.Type))
                issues.Add(new(path, $"Name '{d.Name}' doesn't match type {d.Type}", Severity.Error));

            if ((d.Type is DeviceType.Timer or DeviceType.Counter) && d.Preset is null)
                issues.Add(new(path, $"{d.Type} {d.Name} missing preset", Severity.Warning));

            if (map.ContainsKey(d.Name))
                issues.Add(new(path, $"Duplicate device {d.Name}", Severity.Error));
            else
                map[d.Name] = d;
        }
        return map;
    }

    void ValidateRungs(LadderProgram p, Dictionary<string, Device> devs, List<ValidationIssue> issues)
    {
        for (int r = 0; r < p.Rungs.Count; r++)
        {
            var rung = p.Rungs[r];
            var path = $"$.rungs[{r}]";
            if (rung.Branches.Count == 0)
            {
                issues.Add(new(path, "Rung is empty", Severity.Error)); continue;
            }

            bool sawOutput = false;
            for (int b = 0; b < rung.Branches.Count; b++)
                sawOutput |= ValidateBranch(rung.Branches[b], devs, issues, $"{path}.branches[{b}]");

            if (!sawOutput)
                issues.Add(new(path, "Rung has no output element (Coil/Move/Timer/Counter)", Severity.Error));
        }
    }

    bool ValidateBranch(Branch br, Dictionary<string, Device> devs,
                        List<ValidationIssue> issues, string path)
    {
        bool elementsNull = br.Elements is null;
        bool parallelNull = br.Parallel is null;
        if (elementsNull == parallelNull)
        {
            issues.Add(new(path, "Branch must have exactly one of 'elements' or 'parallel'", Severity.Error));
            return false;
        }

        if (br.Parallel is not null)
        {
            bool any = false;
            for (int i = 0; i < br.Parallel.Count; i++)
                any |= ValidateBranch(br.Parallel[i], devs, issues, $"{path}.parallel[{i}]");
            return any;
        }

        bool hasOutput = false;
        for (int i = 0; i < br.Elements!.Count; i++)
            hasOutput |= ValidateElement(br.Elements[i], devs, issues, $"{path}.elements[{i}]");
        return hasOutput;
    }

    bool ValidateElement(Element e, Dictionary<string, Device> devs,
                         List<ValidationIssue> issues, string path)
    {
        if (!devs.ContainsKey(e.Device))
            issues.Add(new(path, $"Device '{e.Device}' not declared", Severity.Error));

        switch (e.Kind)
        {
            case "Contact":
                return false;
            case "Coil":
                if (e.Action is not ("OUT" or "SET" or "RST"))
                    issues.Add(new(path, "Coil action must be OUT/SET/RST", Severity.Error));
                if (devs.TryGetValue(e.Device, out var cd) &&
                    cd.Type is DeviceType.Input)
                    issues.Add(new(path, "Cannot drive an Input device", Severity.Error));
                return true;
            case "Timer":
            case "Counter":
                if (e.Preset is null or < 0)
                    issues.Add(new(path, $"{e.Kind} needs non-negative preset", Severity.Error));
                return true;
            case "Move":
                if (string.IsNullOrEmpty(e.Source) || string.IsNullOrEmpty(e.Destination))
                    issues.Add(new(path, "Move needs source and destination", Severity.Error));
                return true;
            case "Compare":
                if (e.Operator is null || e.Operand1 is null || e.Operand2 is null)
                    issues.Add(new(path, "Compare needs operator and two operands", Severity.Error));
                return false;
            default:
                issues.Add(new(path, $"Unknown element kind '{e.Kind}'", Severity.Error));
                return false;
        }
    }

    static bool IsPrefixMatch(string name, DeviceType t) => t switch
    {
        DeviceType.Input => name.StartsWith("X", StringComparison.OrdinalIgnoreCase),
        DeviceType.Output => name.StartsWith("Y", StringComparison.OrdinalIgnoreCase),
        DeviceType.Internal => name.StartsWith("M", StringComparison.OrdinalIgnoreCase),
        DeviceType.Timer => name.StartsWith("T", StringComparison.OrdinalIgnoreCase),
        DeviceType.Counter => name.StartsWith("C", StringComparison.OrdinalIgnoreCase),
        DeviceType.DataRegister => name.StartsWith("D", StringComparison.OrdinalIgnoreCase),
        _ => true
    };
}
