using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Backend.Domain.Common.ValueObjects;

[JsonConverter(typeof(EmailJsonConverter))]
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex Pattern = new(
        @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value, string paramName = "value")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty or whitespace.", paramName);

        if (!Pattern.IsMatch(value.Trim()))
            throw new ArgumentException("Email format is invalid. Expected format: user@domain.com", paramName);

        return new Email(value.Trim());
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Email other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;

    public static bool operator ==(Email? left, Email? right) => Equals(left, right);
    public static bool operator !=(Email? left, Email? right) => !Equals(left, right);
}

public sealed class EmailJsonConverter : System.Text.Json.Serialization.JsonConverter<Email>
{
    public override Email? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value is null ? null : Email.Create(value);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, Email value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
