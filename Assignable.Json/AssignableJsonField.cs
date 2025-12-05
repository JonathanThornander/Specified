using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Assignable.Json
{
    /// <summary>
    /// Represents a JSON field that may or may not be assigned a value.
    /// Used for JSON serialization to distinguish between absent fields and null values.
    /// </summary>
    /// <typeparam name="T">The type of the field value.</typeparam>
    [JsonConverter(typeof(AssignableJsonFieldConverterFactory))]
    public readonly struct AssignableJsonField<T>
    {
        /// <summary>
        /// Gets the value of the field, or default if not assigned.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Gets a value indicating whether the field has been assigned a value.
        /// </summary>
        public bool IsAssigned { get; }

        /// <summary>
        /// Indicates whether the value is set and not null.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue => IsAssigned && Value is not null;

        /// <summary>
        /// Initializes a new instance of <see cref="AssignableJsonField{T}"/> with the specified value.
        /// </summary>
        /// <param name="value">The value to assign.</param>
        public AssignableJsonField(T? value)
        {
            IsAssigned = true;
            Value = value;
        }

        /// <summary>
        /// Gets an instance representing an absent (unassigned) field.
        /// </summary>
        public static AssignableJsonField<T> Absent => new(value: default, isAssigned: false);

        /// <summary>
        /// Implicitly converts a value to an <see cref="AssignableJsonField{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator AssignableJsonField<T>(T? value) => new(value, isAssigned: true);

        /// <summary>
        /// Converts this JSON field to an <see cref="Assignable{T}"/> instance.
        /// </summary>
        /// <returns>An <see cref="Assignable{T}"/> representing the same state.</returns>
        public Assignable<T> AsAssignable()
        {
            return IsAssigned
                ? new Assignable<T>(Value)
                : Assignable<T>.Absent();
        }

        /// <summary>
        /// Converts this JSON field to an <see cref="Assignable{TResult}"/> instance, projecting the value using the specified selector.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="selector">A function to project the value.</param>
        /// <returns>An <see cref="Assignable{TResult}"/> representing the projected value or absent.</returns>
        public Assignable<TResult> AsAssignable<TResult>(Func<T?, TResult> selector)
        {
            return IsAssigned
                ? new Assignable<TResult>(selector(Value))
                : Assignable<TResult>.Absent();
        }

        private AssignableJsonField(T? value, bool isAssigned)
        {
            Value = value;
            IsAssigned = isAssigned;
        }
    }
}