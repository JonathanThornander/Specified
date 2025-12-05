using Assignable.AspNetCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Assignable.AspNetCore
{
    /// <summary>
    /// A model binder for <see cref="AssignableQueryParameter{T}"/> that handles binding query parameters
    /// to strongly-typed values. Supports scalar types, collections, nullable types, and special handling
    /// for absent or null values based on configurable options.
    /// </summary>
    /// <typeparam name="T">The type of the value to bind.</typeparam>
    public sealed class AssignableQueryParameterModelBinder<T> : IModelBinder
    {
        private readonly HashSet<string> _nullValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignableQueryParameterModelBinder{T}"/> class.
        /// </summary>
        /// <param name="options">The options containing null value representations.</param>
        /// <summary>
        /// Initializes a new instance of the <see cref="AssignableQueryParameterModelBinder{T}"/> class.
        /// </summary>
        /// <param name="options">The options containing null value representations.</param>
        public AssignableQueryParameterModelBinder(AssignableQueryParameterOptions options)
        {
            _nullValues = options.CreateNullValuesSet();
        }

        /// <summary>
        /// Attempts to bind the model from the current request's query parameters.
        /// If the parameter is absent, returns an absent AssignableQueryParameter.
        /// For collections, parses comma-separated values; for scalars, parses single values.
        /// </summary>
        /// <param name="ctx">The model binding context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task BindModelAsync(ModelBindingContext ctx)
        {
            var name = ctx.ModelName;
            var vp = ctx.ValueProvider.GetValue(name);

            if (vp == ValueProviderResult.None)
            {
                ctx.Result = ModelBindingResult.Success(AssignableQueryParameter<T>.Absent);
                return Task.CompletedTask;
            }

            var targetType = typeof(T);
            if (TryGetEnumerableElementType(targetType, out var elementType))
            {
                return BindEnumerable(ctx, name, vp.Values, targetType, elementType);
            }

            return BindScalar(ctx, name, vp.FirstValue, targetType);
        }

        /// <summary>
        /// Binds a scalar (non-collection) value from the query parameter.
        /// Handles empty strings as absent, configured null values as null,
        /// and attempts to parse the value to the target type.
        /// </summary>
        /// <param name="ctx">The model binding context.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="raw">The raw string value from the query.</param>
        /// <param name="targetType">The target type to bind to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task BindScalar(ModelBindingContext ctx, string name, string? raw, Type targetType)
        {
            if (string.IsNullOrEmpty(raw))
            {
                ctx.Result = ModelBindingResult.Success(AssignableQueryParameter<T>.Absent);
                return Task.CompletedTask;
            }

            if (_nullValues.Contains(raw))
            {
                ctx.Result = ModelBindingResult.Success(new AssignableQueryParameter<T>(default));
                return Task.CompletedTask;
            }

            try
            {
                var parsed = ParseSingle(raw!, targetType);
                if (Nullable.GetUnderlyingType(targetType) == null && parsed is null)
                {
                    ctx.ModelState.AddModelError(name, "Value is required.");
                    ctx.Result = ModelBindingResult.Failed();
                    return Task.CompletedTask;
                }

                ctx.Result = ModelBindingResult.Success(new AssignableQueryParameter<T>((T?)parsed));
            }
            catch
            {
                ctx.ModelState.AddModelError(name, $"Invalid value for {name}.");
                ctx.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Binds a collection value from the query parameter.
        /// Parses comma-separated values from multiple query parameters,
        /// handles null values, and converts to the appropriate collection type.
        /// </summary>
        /// <param name="ctx">The model binding context.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="values">The string values from the query.</param>
        /// <param name="collectionType">The collection type to bind to.</param>
        /// <param name="elementType">The element type of the collection.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task BindEnumerable(ModelBindingContext ctx, string name, StringValues values, Type collectionType, Type elementType)
        {
            if (values.Count == 1 && values[0] is string s && _nullValues.Contains(s))
            {
                ctx.Result = ModelBindingResult.Success(new AssignableQueryParameter<T>(default));
                return Task.CompletedTask;
            }

            // Parse comma-separated values from all query parameters
            var tokens = new List<string>();
            foreach (var v in values)
            {
                if (string.IsNullOrEmpty(v))
                {
                    continue;
                }

                foreach (var part in v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    tokens.Add(part);
                }
            }

            if (tokens.Count == 0)
            {
                object empty = CreateEmptyCollection(collectionType, elementType);
                ctx.Result = ModelBindingResult.Success(new AssignableQueryParameter<T>((T)empty));
                return Task.CompletedTask;
            }

            try
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
                foreach (var t in tokens)
                {
                    list.Add(ParseSingleToElement(t, elementType));
                }

                // Convert list to requested collection type (array, List<T>, IEnumerable<T>, etc.)
                object final = ConvertListToTargetCollection(list, collectionType, elementType);

                ctx.Result = ModelBindingResult.Success(new AssignableQueryParameter<T>((T)final));
            }
            catch
            {
                ctx.ModelState.AddModelError(name, $"Invalid values for {name}.");
                ctx.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines if the given type is an enumerable type and returns its element type.
        /// Handles arrays and types implementing IEnumerable&lt;T&gt;.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="elementType">The element type if enumerable, otherwise null.</param>
        /// <returns>True if the type is enumerable, false otherwise.</returns>
        private static bool TryGetEnumerableElementType(Type type, out Type elementType)
        {
            if (type == typeof(string))
            {
                elementType = null!;
                return false;
            }

            // T[] or IEnumerable<T>/ICollection<T>/IList<T>/List<T>
            if (type.IsArray)
            {
                elementType = type.GetElementType()!;
                return true;
            }

            var ienum = type.GetInterfaces()
                            .Concat(new[] { type })
                            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (ienum != null)
            {
                elementType = ienum.GetGenericArguments()[0];
                return true;
            }

            elementType = null!;
            return false;
        }

        /// <summary>
        /// Creates an empty collection of the specified type.
        /// For arrays, returns an empty array; for interfaces/abstract types, returns a List&lt;T&gt;;
        /// for concrete types, instantiates the type directly.
        /// </summary>
        /// <param name="collectionType">The collection type to create.</param>
        /// <param name="elementType">The element type of the collection.</param>
        /// <returns>An empty collection instance.</returns>
        private static object CreateEmptyCollection(Type collectionType, Type elementType)
        {
            if (collectionType.IsArray)
            {
                return Array.CreateInstance(elementType, 0);
            }

            if (collectionType.IsInterface || collectionType.IsAbstract)
            {
                // fall back to List<T> for IEnumerable<T>/IList<T>/ICollection<T>
                var listType = typeof(List<>).MakeGenericType(elementType);
                return Activator.CreateInstance(listType)!;
            }

            // Concrete type with default ctor (e.g., List<T>)
            return Activator.CreateInstance(collectionType)!;
        }

        /// <summary>
        /// Converts a List&lt;elementType&gt; to the target collection type.
        /// Handles arrays, interfaces (by returning List&lt;T&gt;), and concrete collections with Add methods.
        /// Falls back to array if conversion fails.
        /// </summary>
        /// <param name="list">The list to convert.</param>
        /// <param name="collectionType">The target collection type.</param>
        /// <param name="elementType">The element type.</param>
        /// <returns>The converted collection.</returns>
        private static object ConvertListToTargetCollection(IList list, Type collectionType, Type elementType)
        {
            if (collectionType.IsArray)
            {
                var array = Array.CreateInstance(elementType, list.Count);
                list.CopyTo(array, 0);
                return array;
            }

            if (collectionType.IsInterface || collectionType.IsAbstract)
            {
                // IEnumerable<T>/ICollection<T>/IList<T> -> return List<T>
                var listType = typeof(List<>).MakeGenericType(elementType);
                var concrete = Activator.CreateInstance(listType)!;
                foreach (var item in list) ((IList)concrete).Add(item);
                return concrete;
            }

            // Try to new up the requested concrete collection and add items
            var target = Activator.CreateInstance(collectionType);
            var add = collectionType.GetMethod("Add", new[] { elementType });
            if (target != null && add != null)
            {
                foreach (var item in list) add.Invoke(target, new[] { item });
                return target;
            }

            // Fallback: return array
            var arr = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(arr, 0);
            return arr;
        }

        /// <summary>
        /// Parses a single string value to the specified target type.
        /// Handles common types like string, char, bool, Guid, DateTime, enums, and uses TypeDescriptor for others.
        /// </summary>
        /// <param name="raw">The raw string value to parse.</param>
        /// <param name="targetType">The target type to convert to.</param>
        /// <returns>The parsed object, or null if parsing fails.</returns>
        /// <exception cref="FormatException">Thrown when parsing fails for certain types.</exception>
        private static object? ParseSingle(string raw, Type targetType)
        {
            // Handle string types immediately - no conversion needed
            if (targetType == typeof(string))
            {
                return raw;
            }

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Double-check for string after unwrapping nullable
            if (underlying == typeof(string))
            {
                return raw;
            }

            // Handle char - convert string to char (take first character)
            if (underlying == typeof(char))
            {
                if (string.IsNullOrEmpty(raw))
                {
                    throw new FormatException("Cannot convert empty string to char.");
                }
                return raw[0];
            }

            if (underlying == typeof(bool))
            {
                // Support 1/0 plus true/false; treat "1" as true, "0" as false, others parse normally
                return raw is "1" || raw is not "0" && bool.Parse(raw);
            }

            if (underlying == typeof(Guid))
            {
                return Guid.Parse(raw);
            }

            if (underlying == typeof(DateTime))
            {
                // First try strict yyyy-MM-dd as UTC date; then fallback to culture-aware
                if (!DateTime.TryParseExact(raw, "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var dt))
                {
                    if (!DateTime.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                    {
                        throw new FormatException();
                    }
                }
                return dt;
            }

            if (underlying.GetTypeInfo().IsEnum)
            {
                return Enum.Parse(underlying, raw, ignoreCase: true);
            }

            var conv = TypeDescriptor.GetConverter(underlying);
            return conv.ConvertFromInvariantString(raw);
        }

        /// <summary>
        /// Parses a single string value to the element type, handling nullable types.
        /// Throws if parsing results in null for non-nullable types.
        /// </summary>
        /// <param name="raw">The raw string value to parse.</param>
        /// <param name="elementType">The element type to convert to.</param>
        /// <returns>The parsed object.</returns>
        /// <exception cref="FormatException">Thrown when parsing fails or null is returned for non-nullable types.</exception>
        private static object ParseSingleToElement(string raw, Type elementType)
        {
            var u = Nullable.GetUnderlyingType(elementType) ?? elementType;
            var val = ParseSingle(raw, u);

            if (val == null && Nullable.GetUnderlyingType(elementType) == null)
            {
                throw new FormatException();
            }

            return val!;
        }
    }
}