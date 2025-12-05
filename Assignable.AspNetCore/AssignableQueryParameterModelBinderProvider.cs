using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Assignable.AspNetCore
{
    /// <summary>
    /// Provides model binders for <see cref="AssignableQueryParameter{T}"/> types.
    /// </summary>
    public sealed class AssignableQueryParameterModelBinderProvider : IModelBinderProvider
    {
        private readonly AssignableQueryParameterOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignableQueryParameterModelBinderProvider"/> class.
        /// </summary>
        /// <param name="options">The options for query parameter binding.</param>
        public AssignableQueryParameterModelBinderProvider(AssignableQueryParameterOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Gets a model binder for the specified model type if it is an <see cref="AssignableQueryParameter{T}"/>.
        /// </summary>
        /// <param name="context">The model binder provider context.</param>
        /// <returns>A model binder instance, or null if the type is not supported.</returns>
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            var mt = context.Metadata.ModelType;
            if (mt.IsGenericType && mt.GetGenericTypeDefinition() == typeof(AssignableQueryParameter<>))
            {
                var inner = mt.GetGenericArguments()[0];
                var binderType = typeof(AssignableQueryParameterModelBinder<>).MakeGenericType(inner);
                return (IModelBinder)Activator.CreateInstance(binderType, _options)!;
            }
            return null;
        }
    }
}