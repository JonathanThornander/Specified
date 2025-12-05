namespace Assignable.AspNetCore
{
    /// <summary>
    /// Configuration options for Assignable query parameter binding in ASP.NET Core.
    /// </summary>
    public class AssignableQueryParameterOptions
    {
        /// <summary>
        /// Gets or sets the string values that should be interpreted as null in query parameters.
        /// Default values are "null" and "nil" (case-insensitive).
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
