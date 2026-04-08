namespace Blinder.SharedKernel;

/// <summary>
/// Strongly-typed wrapper for request correlation identifiers.
/// Used for distributed tracing and log correlation across services.
/// </summary>
public readonly struct CorrelationId : IEquatable<CorrelationId>
{
    public Guid Value { get; }

    public CorrelationId(Guid value)
    {
        Value = value;
    }

    public static CorrelationId New() => new(Guid.NewGuid());

    public static CorrelationId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string? value, out CorrelationId correlationId)
    {
        if (Guid.TryParse(value, out var guid))
        {
            correlationId = new CorrelationId(guid);
            return true;
        }
        correlationId = default;
        return false;
    }

    public bool Equals(CorrelationId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is CorrelationId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(CorrelationId left, CorrelationId right) => left.Equals(right);
    public static bool operator !=(CorrelationId left, CorrelationId right) => !left.Equals(right);
}
