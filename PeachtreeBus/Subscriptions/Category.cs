using System.Text.Json.Serialization;

namespace PeachtreeBus.Subscriptions;

[JsonConverter(typeof(CategoryJsonConverter))]
public readonly record struct Category
{
    public const int MaxLength = 128;
    private readonly string _value;

    public string Value => _value
        ?? throw new CategoryException("Category is not initialized.");

    public Category(string value)
    {
        CategoryException.ThrowIfInvalid(value);
        _value = value;
    }

    public override string ToString() => Value;

    public class CategoryJsonConverter()
        : PeachtreeBusJsonConverter<Category, string>(v => new(v!), v => v.Value);
}

public class CategoryException(string message) : PeachtreeBusException(message)
{
    public static void ThrowIfInvalid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new CategoryException(
                "A SagaKey cannot be null or whitespace.");

        if (value.Length > Category.MaxLength)
            throw new CategoryException(
                $"A SagaKey has a max length of {Category.MaxLength} characters.");
    }
}

