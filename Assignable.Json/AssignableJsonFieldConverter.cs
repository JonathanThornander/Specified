using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assignable.Json
{
    /// <summary>
    /// JSON converter for <see cref="AssignableJsonField{T}"/> that handles serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The type of the field value.</typeparam>
    public class AssignableJsonFieldConverter<T> : JsonConverter<AssignableJsonField<T>>
    {
        private readonly HashSet<string> _nullValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignableJsonFieldConverter{T}"/> class.
        /// </summary>
        /// <param name="options">The options for JSON field handling.</param>
        public AssignableJsonFieldConverter(AssignableJsonOptions options)
        {
            _nullValues = options.CreateNullValuesSet();
        }

        /// <summary>
        /// Reads and converts the JSON to an <see cref="AssignableJsonField{T}"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The converted value.</returns>
        public override AssignableJsonField<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new AssignableJsonField<T>(default);
            }

            // Check if string value matches one of the null values
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (stringValue != null && _nullValues.Contains(stringValue))
                {
                    return new AssignableJsonField<T>(default);
                }
            }

            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            return new AssignableJsonField<T>(value);
        }

        /// <summary>
        /// Writes an <see cref="AssignableJsonField{T}"/> as JSON.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, AssignableJsonField<T> value, JsonSerializerOptions options)
        {
            if (!value.IsAssigned)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}