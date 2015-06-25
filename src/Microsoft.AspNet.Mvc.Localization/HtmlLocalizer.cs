// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
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
        private IStringLocalizer _localizer;
        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly IHtmlEncoder _encoder;

        /// <summary>
        /// Creates a new <see cref="HtmlLocalizer"/>.
        /// </summary>
        /// <param name="factory">The <see cref="IStringLocalizerFactory"/>.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/>.</param>
        public HtmlLocalizer([NotNull] IStringLocalizerFactory factory, [NotNull] IHtmlEncoder encoder)
        {
            _localizerFactory = factory;
            _encoder = encoder;
        }

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

        /// <inheritdoc />
        public virtual LocalizedString this[[NotNull] string key] => _localizer[key];

        /// <inheritdoc />
        public virtual LocalizedString this[[NotNull] string key, params object[] arguments] =>
            _localizer[key, arguments];

        /// <summary>
        /// Creates a new <see cref="IStringLocalizer"/> for a specific <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to use.</param>
        /// <returns>A culture-specific <see cref="IStringLocalizer"/>.</returns>
        IStringLocalizer IStringLocalizer.WithCulture([NotNull] CultureInfo culture) => WithCulture(culture);

        /// <inheritdoc />
        public virtual LocalizedString GetString([NotNull] string key) => _localizer.GetString(key);

        /// <inheritdoc />
        public virtual LocalizedString GetString([NotNull] string key, params object[] arguments) =>
            _localizer.GetString(key, arguments);

        /// <inheritdoc />
        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures) =>
            _localizer.GetAllStrings(includeAncestorCultures);

        /// <inheritdoc />
        public virtual LocalizedHtmlString Html([NotNull] string key) => ToHtmlString(_localizer.GetString(key));

        /// <inheritdoc />
        public virtual LocalizedHtmlString Html([NotNull] string key, params object[] arguments)
        {
            var stringValue = _localizer[key].Value;
            var tokens = new List<string>();

            for (int i = 0; i < stringValue.Length; i++)
            {
                if (stringValue[i] == '{')
                {
                    StringBuilder myToken = new StringBuilder();
                    myToken.Append(stringValue[i]);
                    i++;

                    while (i < stringValue.Length)
                    {
                        if (stringValue[i] != '}')
                        {
                            if (stringValue[i] == '{')
                            {
                                throw new Exception("Malformed resource string");
                            }
                            else
                            {
                                myToken.Append(stringValue[i]);
                                i++;
                            }
                        }
                        else
                        {
                            myToken.Append(stringValue[i]);
                            tokens.Add(myToken.ToString());
                            break;
                        }
                    }
                }
            }
            // This call joins all tokens and formats them based on arguments and splits them again into arguments. The
            // idea here is to encode the formatted arguments. For example if resource value has datetime format as
            // argument ex: {1:yyyy} we are formatting the argument to right value before encoding.
            arguments = string.Format(string.Join(",", tokens), arguments).Split(',');

            return ToHtmlString(new LocalizedString(key, string.Format(stringValue, EncodeArguments(arguments))));
        }

        /// <summary>
        /// Creates a new <see cref="IHtmlLocalizer"/> for a specific <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to use.</param>
        /// <returns>A culture-specific <see cref="IHtmlLocalizer"/>.</returns>
        public IHtmlLocalizer WithCulture([NotNull] CultureInfo culture) =>
            new HtmlLocalizer(_localizer.WithCulture(culture), _encoder);

        /// <summary>
        /// Creates a new <see cref="LocalizedHtmlString"/> for a <see cref="LocalizedString"/>.
        /// </summary>
        /// <param name="result">The <see cref="LocalizedString"/>.</param>
        protected LocalizedHtmlString ToHtmlString(LocalizedString result) =>
            new LocalizedHtmlString(result.Name, result.Value, result.ResourceNotFound);

        /// <summary>
        /// Encodes the arguments based on the object type.
        /// </summary>
        /// <param name="arguments">The array of objects to encode.</param>
        /// <returns>The encoded object array.</returns>
        protected object[] EncodeArguments(object[] arguments)
        {
            var encodedArguments = new object[arguments.Length];
            for (var index = 0; index < arguments.Length; ++index)
            {
                var argument = arguments[index];
                if (argument is HtmlString || argument == null)
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

        /// <summary>
        /// Creates a <see cref="IStringLocalizer"/>.
        /// </summary>
        /// <param name="baseName">The base name of the resource to load strings from.</param>
        /// <param name="location">The location to load resources from.</param>
        /// <returns>The <see cref="IStringLocalizer"/>.</returns>
        protected void CreateStringLocalizer(string baseName, string location) =>
            _localizer = _localizerFactory.Create(baseName, location);

        /// <summary>
        /// Creates a <see cref="IStringLocalizer"/> using the <see cref="System.Reflection.Assembly"/> and
        /// <see cref="Type.FullName"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        protected void CreateStringLocalizer(Type type) => _localizer = _localizerFactory.Create(type);
    }
}