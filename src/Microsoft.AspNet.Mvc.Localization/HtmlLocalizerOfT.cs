// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Framework.Localization;

namespace Microsoft.AspNet.Mvc.Localization
{
    /// <summary>
    /// Represents an <see cref="IHtmlLocalizer"/> that provides strings for <see cref="TResourceSource"/>.
    /// </summary>
    /// <typeparam name="TResourceSource">The <see cref="System.Type"/> to provide strings for.</typeparam>
    public class HtmlLocalizer<TResourceSource> : IHtmlLocalizer<TResourceSource>
    {
        private readonly IHtmlLocalizer _localizer;

        public HtmlLocalizer(IHtmlLocalizerFactory factory)
        {
            _localizer = factory.Create(typeof(TResourceSource));
        }

        /// <inheritdoc />
        public virtual IHtmlLocalizer WithCulture(CultureInfo culture) => _localizer.WithCulture(culture);

        /// <inheritdoc />
        IStringLocalizer IStringLocalizer.WithCulture(CultureInfo culture) => _localizer.WithCulture(culture);

        /// <inheritdoc />
        public virtual LocalizedString this[string key] => _localizer[key];

        /// <inheritdoc />
        public virtual LocalizedString this[string key, params object[] arguments] => _localizer[key, arguments];

        /// <inheritdoc />
        public virtual LocalizedString GetString(string key) => _localizer.GetString(key);

        /// <inheritdoc />
        public virtual LocalizedString GetString(string key, params object[] arguments)
            => _localizer.GetString(key, arguments);

        /// <inheritdoc />
        public virtual LocalizedHtmlString Html(string key) => _localizer.Html(key);

        /// <inheritdoc />
        public virtual LocalizedHtmlString Html(string key, params object[] arguments)
            => _localizer.Html(key, arguments);

        /// <inheritdoc />
        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
            => _localizer.GetAllStrings(includeAncestorCultures);
    }
}