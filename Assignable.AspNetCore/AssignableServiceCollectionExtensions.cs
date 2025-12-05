using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Assignable.AspNetCore
{
    /// <summary>
    /// Extension methods for configuring Assignable query parameter support in ASP.NET Core.
    /// </summary>
    public static class AssignableServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Assignable query parameter model binding support to MVC.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Optional action to configure the options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddAssignableQueryParameters(
            this IServiceCollection services,
            Action<AssignableQueryParameterOptions>? configureOptions = null)
        {
            var options = new AssignableQueryParameterOptions();
            configureOptions?.Invoke(options);

            services.Configure<MvcOptions>(mvcOptions =>
            {
                mvcOptions.ModelBinderProviders.Insert(0, new AssignableQueryParameterModelBinderProvider(options));
            });

            return services;
        }
    }
}
