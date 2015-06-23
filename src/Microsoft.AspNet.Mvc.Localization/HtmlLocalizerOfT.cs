// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Internal;
using Microsoft.Framework.Localization;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.Localization
{
    /// <summary>
    /// This is an <see cref="IHtmlLocalizer"/> that provides strings for <see cref="TResource"/>.
    /// </summary>
    /// <typeparam name = "TResource"> The <see cref="System.Type"/> to scope the resource names.</typeparam>
    public class HtmlLocalizer<TResource> : HtmlLocalizer, IHtmlLocalizer<TResource>
    {
        /// <summary>
        /// Creates a new <see cref="HtmlLocalizer" for <see cref="TResource"/>.
        /// </summary>
        /// <param name="factory">The <see cref="IStringLocalizerFactory"/>.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/>.</param>
        public HtmlLocalizer(
            [NotNull] IStringLocalizerFactory factory,
            [NotNull]IHtmlEncoder encoder) 
            : base(factory, encoder)
        {
            CreateStringLocalizer(typeof(TResource));
        }
    }
}