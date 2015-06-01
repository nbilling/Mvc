// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Mvc.Localization;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.Framework.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Mvc localization to the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMvcLocalization([NotNull] this IServiceCollection services)
        {
            return AddMvcLocalization(services, LanguageViewLocationExpanderOption.Suffix);
        }

        /// <summary>
        ///  Adds Mvc localization to the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="option">The view format for localized views.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMvcLocalization(
            [NotNull] this IServiceCollection services,
            LanguageViewLocationExpanderOption option)
        {
            services.ConfigureRazorViewEngine(options =>
            {
                options.ViewLocationExpanders.Add(new LanguageViewLocationExpander(option));
            });

            services.AddSingleton<IHtmlLocalizerFactory, HtmlLocalizerFactory>();
            services.AddTransient(typeof(IHtmlLocalizer<>), typeof(HtmlLocalizer<>));
            services.AddTransient(typeof(IViewLocalizer), typeof(ViewLocalizer));
            if (!services.Any(sd => sd.ServiceType == typeof(IHtmlEncoder)))
            {
                services.AddInstance<IHtmlEncoder>(HtmlEncoder.Default);
            }
            return services.AddLocalization();
        }
    }
}