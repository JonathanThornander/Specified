using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assignable.Json
{
    /// <summary>
    /// Factory for creating <see cref="AssignableJsonFieldConverter{T}"/> instances.
    /// </summary>
    public class AssignableJsonFieldConverterFactory : JsonConverterFactory
    {
        private readonly AssignableJsonOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignableJsonFieldConverterFactory"/> class with default options.
        /// </summary>
        public AssignableJsonFieldConverterFactory()
            : this(new AssignableJsonOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignableJsonFieldConverterFactory"/> class.
        /// </summary>
        /// <param name="options">The options for JSON field handling.</param>
        public AssignableJsonFieldConverterFactory(AssignableJsonOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Determines whether the factory can convert the specified type.
        /// </summary>
        /// <param name="typeToConvert">The type to check.</param>
        /// <returns>True if the type is <see cref="AssignableJsonField{T}"/>, otherwise false.</returns>
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(AssignableJsonField<>);

        /// <summary>
        /// Creates a converter for the specified type.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>A JSON converter instance.</returns>
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            var innerType = type.GetGenericArguments()[0];
            var converterType = typeof(AssignableJsonFieldConverter<>).MakeGenericType(innerType);
            return (JsonConverter)Activator.CreateInstance(converterType, _options)!;
        }
    }
}