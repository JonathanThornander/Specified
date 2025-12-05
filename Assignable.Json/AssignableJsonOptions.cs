namespace Assignable.Json
{
    /// <summary>
    /// Configuration options for Assignable JSON field serialization.
    /// </summary>
    public class AssignableJsonOptions
    {
        /// <summary>
        /// Gets or sets the string values that should be interpreted as null when deserializing.
        /// Default values are "null" and "nil" (case-insensitive).
        /// Note: This applies to string values in JSON, not the JSON null token.
        /// </summary>
        public IReadOnlyCollection<string> NullValues { get; set; } = AssignableOptions.DefaultNullValues;

        /// <summary>
        /// Creates a HashSet from the configured null values for efficient lookups.
        /// </summary>
        internal HashSet<string> CreateNullValuesSet()
        {
            return new HashSet<string>(NullValues, StringComparer.OrdinalIgnoreCase);
        }
    }
}
