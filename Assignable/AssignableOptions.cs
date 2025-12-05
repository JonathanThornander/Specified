namespace Assignable
{
    /// <summary>
    /// Configuration options for Assignable behavior.
    /// </summary>
    public class AssignableOptions
    {
        /// <summary>
        /// Gets or sets the string values that should be interpreted as null.
        /// Default values are "null", "nil", and "NULL" (case-insensitive).
        /// </summary>
        public IReadOnlyCollection<string> NullValues { get; set; } = DefaultNullValues;

        /// <summary>
        /// Gets the default null value strings.
        /// </summary>
        public static IReadOnlyCollection<string> DefaultNullValues { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "null",
            "nil"
        };

        /// <summary>
        /// Creates a HashSet from the configured null values for efficient lookups.
        /// </summary>
        public HashSet<string> CreateNullValuesSet()
        {
            return new HashSet<string>(NullValues, StringComparer.OrdinalIgnoreCase);
        }
    }
}
