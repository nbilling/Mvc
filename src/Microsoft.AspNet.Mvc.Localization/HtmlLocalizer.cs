// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Localization;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.Localization
{
    /// <summary>
    /// An <see cref="IHtmlLocalizer"/> that uses the <see cref="IStringLocalizer"/> to provide localized HTML content.
    /// </summary>
    public class HtmlLocalizer : IHtmlLocalizer
    {
        private readonly IStringLocalizer _localizer;
        private readonly IHtmlEncoder _encoder;

        /// <summary>
        /// Creates a new <see cref="HtmlLocalizer"/>.
        /// </summary>
        /// <param name="localizer">The <see cref="IStringLocalizer"/> to read strings from.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/>.</param>
        public HtmlLocalizer([NotNull] IStringLocalizer localizer, [NotNull] IHtmlEncoder encoder)
        {
            _localizer = localizer;
            _encoder = encoder;
        }

        /// <summary>
        /// Creates a new <see cref="HtmlLocalizer"/> for a specific <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to use.</param>
        /// <returns>A culture-specific <see cref="HtmlLocalizer"/>.</returns>
        public virtual IHtmlLocalizer WithCulture([NotNull] CultureInfo culture) =>
            new HtmlLocalizer(_localizer.WithCulture(culture), _encoder);

        IStringLocalizer IStringLocalizer.WithCulture([NotNull] CultureInfo culture) => WithCulture(culture);

        /// <inheritdoc />
        public virtual LocalizedString this[string key] => _localizer[key];

        /// <inheritdoc />
        public virtual LocalizedString this[string key, params object[] arguments] => _localizer[key, arguments];

        /// <inheritdoc />
        public virtual LocalizedString GetString([NotNull] string key) => _localizer.GetString(key);

        /// <inheritdoc />
        public virtual LocalizedString GetString([NotNull] string key, params object[] arguments) =>
            _localizer.GetString(key, arguments);

        /// <inheritdoc />
        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)=>
            _localizer.GetAllStrings(includeAncestorCultures);

        /// <inheritdoc />
        public virtual LocalizedHtmlString Html([NotNull] string key) => ToHtmlString(_localizer.GetString(key));

        /// <inheritdoc />
        public virtual LocalizedHtmlString Html([NotNull] string key, params object[] arguments)
        {
            return ToHtmlString(_localizer.GetString(key, EncodeArguments(arguments)));
        }

        protected LocalizedHtmlString ToHtmlString(LocalizedString result)
        {
            return new LocalizedHtmlString(result.Name, result.Value, result.ResourceNotFound);
        }

        protected object[] EncodeArguments(object[] arguments)
        {
            object[] encodedArguments = new object[arguments.Length];
            for (var index = 0; index != arguments.Length; ++index)
            {
                var argument = arguments[index];
                if (argument.GetType().GetTypeInfo().IsPrimitive ||
                    argument is HtmlString ||
                    argument is DateTime ||
                    argument is DateTimeOffset)
                {
                    encodedArguments[index] = argument;
                }
                else
                {
                    encodedArguments[index] = _encoder.HtmlEncode(argument.ToString());
                }
            }
            return encodedArguments;
        }
    }
}