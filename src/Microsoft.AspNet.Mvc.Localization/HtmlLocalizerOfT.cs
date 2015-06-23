// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Localization;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.Localization
{
    /// <summary>
    /// This is an <see cref="IHtmlLocalizer"/> that provides strings for <see cref="TResourceSource"/>.
    /// </summary>
    /// <typeparam name="TResourceSource">The <see cref="System.Type"/> to provide strings for.</typeparam>
    public class HtmlLocalizer<TResourceSource> : HtmlLocalizer, IHtmlLocalizer<TResourceSource>
    {
        /// <summary>
        /// Creates a new <see cref="HtmlLocalizer" for <see cref="TResourceSource"/>.
        /// </summary>
        /// <param name="factory">The <see cref="IStringLocalizerFactory"/>.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/>.</param>
        public HtmlLocalizer(
            [NotNull] IStringLocalizerFactory factory,
            [NotNull]IHtmlEncoder encoder) 
            : base(factory, encoder)
        {
            CreateStringLocalizer(typeof(TResourceSource));
        }
    }
}