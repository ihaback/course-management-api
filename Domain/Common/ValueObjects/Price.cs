using System.Text.Json.Serialization;

namespace Backend.Domain.Common.ValueObjects;

[JsonConverter(typeof(PriceJsonConverter))]
public sealed class Price : IEquatable<Price>
{
    public decimal Value { get; }

    private Price(decimal value) => Value = value;

    public static Price Create(decimal value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Price cannot be negative.");

        return new Price(value);
    }

    public bool Equals(Price? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Price other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString("G");

    public static bool operator ==(Price? left, Price? right) => Equals(left, right);
    public static bool operator !=(Price? left, Price? right) => !Equals(left, right);
}

public sealed class PriceJsonConverter : System.Text.Json.Serialization.JsonConverter<Price>
{
    public override Price? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        if (reader.TokenType == System.Text.Json.JsonTokenType.Null)
            return null;
        return Price.Create(reader.GetDecimal());
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, Price value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}
