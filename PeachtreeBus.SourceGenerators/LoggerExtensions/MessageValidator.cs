using System;
using System.Text.RegularExpressions;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IMessageValidator
{
    void Validate(string value);
}

public class MessageValidator : IMessageValidator
{
    private static readonly Regex unescapedQuote = new("(\\\"(?<=[^\\\\]\\\")|^\\\")");
    private static readonly Regex untypedReplacement = new("{[^\\:]*}");

    public void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApplicationException("Event message is null or whitespace.");
        }

        var matches = unescapedQuote.Matches(value);
        if (matches.Count > 0)
        {
            throw new ApplicationException("Message contains unescaped quotes. " + value);
        }

        matches = untypedReplacement.Matches(value);
        if (matches.Count > 0)
        {
            throw new ApplicationException("Message contains untyped replacement" + value);
        }
    }
}