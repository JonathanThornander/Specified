namespace Assignable.AspNetCore
{
    /// <summary>
    /// Represents a query parameter that may or may not be assigned a value.
    /// Used in ASP.NET Core model binding to distinguish between absent parameters and null values.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    public readonly struct AssignableQueryParameter<T>
    {
        /// <summary>
        /// Gets a value indicating whether the parameter has been assigned a value.
        /// </summary>
        public bool IsAssigned { get; }

        /// <summary>
        /// Gets the value of the parameter, or default if not assigned.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="AssignableQueryParameter{T}"/> with the specified value.
        /// </summary>
        /// <param name="value">The value to assign.</param>
        public AssignableQueryParameter(T? value)
        {
            IsAssigned = true;
            Value = value;
        }

        /// <summary>
        /// Gets an instance representing an absent (unassigned) parameter.
        /// </summary>
        public static AssignableQueryParameter<T> Absent => new(value: default, isAssigned: false);

        /// <summary>
        /// Implicitly converts a value to an <see cref="AssignableQueryParameter{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator AssignableQueryParameter<T>(T? value) => new(value, true);

        /// <summary>
        /// Converts this query parameter to an <see cref="Assignable{T}"/> instance.
        /// </summary>
        /// <returns>An <see cref="Assignable{T}"/> representing the same state.</returns>
        public Assignable<T> AsAssignable()
        {
            return IsAssigned
                ? new Assignable<T>(Value)
                : Assignable<T>.Absent();
        }

        private AssignableQueryParameter(T? value, bool isAssigned)
        {
            Value = value;
            IsAssigned = isAssigned;
        }
    }
}