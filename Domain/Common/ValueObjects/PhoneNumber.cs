using System.Text.Json.Serialization;

namespace Backend.Domain.Common.ValueObjects;

[JsonConverter(typeof(PhoneNumberJsonConverter))]
public sealed class PhoneNumber : IEquatable<PhoneNumber>
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value, string paramName = "value")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty or whitespace.", paramName);

        return new PhoneNumber(value.Trim());
    }

    public bool Equals(PhoneNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PhoneNumber other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right) => Equals(left, right);
    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !Equals(left, right);
}

public sealed class PhoneNumberJsonConverter : System.Text.Json.Serialization.JsonConverter<PhoneNumber>
{
    public override PhoneNumber? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value is null ? null : PhoneNumber.Create(value);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, PhoneNumber value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
