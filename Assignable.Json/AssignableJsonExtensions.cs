using System.Text.Json;

namespace Assignable.Json
{
    /// <summary>
    /// Extension methods for configuring Assignable JSON serialization support.
    /// </summary>
    public static class AssignableJsonExtensions
    {
        /// <summary>
        /// Adds Assignable JSON field converter to the JsonSerializerOptions.
        /// </summary>
        /// <param name="options">The JsonSerializerOptions to configure.</param>
        /// <param name="configureOptions">Optional action to configure the Assignable options.</param>
        /// <returns>The JsonSerializerOptions for chaining.</returns>
        public static JsonSerializerOptions AddAssignableJsonFields(
            this JsonSerializerOptions options,
            Action<AssignableJsonOptions>? configureOptions = null)
        {
            var assignableOptions = new AssignableJsonOptions();
            configureOptions?.Invoke(assignableOptions);

            options.Converters.Add(new AssignableJsonFieldConverterFactory(assignableOptions));
            return options;
        }
    }
}
