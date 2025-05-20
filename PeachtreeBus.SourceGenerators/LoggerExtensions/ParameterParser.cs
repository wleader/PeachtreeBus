using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public class LogParameter
{
    public string Substitution { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string LowerName { get; set; } = default!;
    public string TypeName { get; set; } = default!;
}

public interface IParameterParser
{
    List<LogParameter> Parse(string message);
}

public class ParameterParser : IParameterParser
{
    private static readonly Regex regex = new("{[^:]*:[^}]*}");

    public List<LogParameter> Parse(string message)
    {
        List<LogParameter> result = [];
        foreach (Match match in regex.Matches(message))
        {
            var value = match.Value;
            var colonPos = value.IndexOf(':');
            var name = value.Substring(1, colonPos - 1);
            var typeName = value.Substring(colonPos + 1, value.Length - colonPos - 2);
            var lowerName = name.Length == 1
                ? char.ToLower(name[0]).ToString()
                : char.ToLower(name[0]) + name.Substring(1);

            result.Add(new()
            {
                Substitution = value,
                Name = name,
                LowerName = lowerName,
                TypeName = typeName,
            });
        }
        return result;
    }
}
