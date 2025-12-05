using System.Diagnostics.CodeAnalysis;

namespace Assignable
{
    public readonly struct Assignable<T>
    {
        public T? Value { get; }

        /// <summary>
        /// Indicates whether the value is set, no matter if it is null or not.
        /// </summary>
        public bool IsAssigned { get; }

        /// <summary>
        /// Indicates whether the value is set and not null.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue => IsAssigned && Value is not null;

        /// <summary>
        /// Creates an instance of <see cref="Assignable{T}"/> that represents an unassigned value.
        /// </summary>
        /// <remarks>Use this method to explicitly indicate the absence of a value when working with <see
        /// cref="Assignable{T}"/>. The returned instance will have <c>IsAssigned</c> set to <see langword="false"/> and
        /// its <c>Value</c> property set to the default value of type <typeparamref name="T"/>.</remarks>
        /// <returns>An <see cref="Assignable{T}"/> instance with no value assigned.</returns>
        public static Assignable<T> Absent() => new(value: default, isAssigned: false);

        /// <summary>
        /// Initializes a new instance of the Assignable class with the specified value, marking it as assigned.
        /// </summary>
        /// <param name="value">The value to assign to the instance. May be null for reference or nullable types.</param>
        public Assignable(T? value) : this(value, isAssigned: true) { }

        /// <summary>
        /// Implicitly converts a value of type <typeparamref name="T"/> to an <see cref="Assignable{T}"/> instance.
        /// </summary>
        /// <param name="value">The value to be wrapped in an <see cref="Assignable{T}"/>. Can be <see langword="null"/> if <typeparamref
        /// name="T"/> is a reference type or a nullable value type.</param>
        public static implicit operator Assignable<T>(T? value) => new(value);

        private Assignable(T? value, bool isAssigned)
        {
            Value = value;
            IsAssigned = isAssigned;
        }
    }
}
