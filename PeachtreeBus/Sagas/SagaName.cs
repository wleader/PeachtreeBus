using PeachtreeBus.Data;

namespace PeachtreeBus.Sagas;

public readonly record struct SagaName
{
    public string Value { get; }

    public SagaName(string value)
    {
        DbSafeNameException.ThrowIfNotSafe(value, nameof(SagaName));
        Value = value;
    }

    public override string ToString() => Value
        ?? throw new DbSafeNameException($"{nameof(SagaName)} is not initialized.");
}
