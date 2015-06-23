// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Localization;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.Localization
{
    /// <summary>
    /// A <see cref="HtmlLocalizer"/> that provides localized strings for views.
    /// </summary>
    public class ViewLocalizer : HtmlLocalizer, ICanHasViewContext
    {
        private readonly string _applicationName;

        /// <summary>
        /// Creates a new <see cref="ViewLocalizer"/>.
        /// </summary>
        /// <param name="localizerFactory">The <see cref="IStringLocalizerFactory"/>.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/>.</param>
        /// <param name="applicationEnvironment">The <see cref="IApplicationEnvironment"/>.</param>
        public ViewLocalizer(
            [NotNull] IStringLocalizerFactory localizerFactory,
            [NotNull] IHtmlEncoder encoder,
            [NotNull] IApplicationEnvironment applicationEnvironment)
            : base(localizerFactory, encoder)
        {
            _applicationName = applicationEnvironment.ApplicationName;
        }

        public void Contextualize(ViewContext viewContext)
        {
            var baseName = viewContext.View.Path.Replace('/', '.').Replace('\\', '.');
            if (baseName.StartsWith("."))
            {
                baseName = baseName.Substring(1);
            }
            baseName = _applicationName + "." + baseName;
            CreateStringLocalizer(baseName, _applicationName); 
        }
    }
}