// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Localization;
using Microsoft.Framework.WebEncoders;


namespace Microsoft.AspNet.Mvc.Localization
{
    /// <summary>
    /// An <see cref="IHtmlLocalizer"/> that uses the <see cref="IStringLocalizer"/> to provide localized HTML content.
    /// This service just encodes the arguments but not the resource string.
    /// </summary>
    public class HtmlLocalizer : IHtmlLocalizer
    {
        private IStringLocalizer _localizer;
        private IStringLocalizerFactory _localizerFactory;
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

        private IStringLocalizer Localizer
        {
            get
            {
                if (_localizer == null)
                {
                    throw new InvalidOperationException(Resources.NullStringLocalizer);
                }

                return _localizer;
            }
            [param: NotNull]
            set
            {
                _localizer = value;
            }
        }

        private IStringLocalizerFactory LocalizerFactory
        {
            get
            {
                if (_localizerFactory == null)
                {
                    throw new InvalidOperationException(Resources.NullStringLocalizerFactory);
                }

                return _localizerFactory;
            }
            [param: NotNull]
            set
            {
                _localizerFactory = value;
            }
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

            return ToHtmlString(new LocalizedString(key, EncodeArguments(stringValue, arguments)));
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

        /// <summary>
        /// Encodes the arguments based on the object type.
        /// </summary>
        /// <param name="resourceString">The resourceString whose arguments need to be encoded.</param>
        /// <param name="arguments">The array of objects to encode.</param>
        /// <returns>The string with encoded arguments.</returns>
        protected string EncodeArguments([NotNull] string resourceString, [NotNull] object[] arguments)
        {
            var position = 0;
            var length = resourceString.Length;
            var currentCharacter = '\x0';
            StringBuilder tokenBuffer = null;
            var outputBuffer = new StringBuilder();
            var isToken = false;

            while (position < length)
            {
                currentCharacter = resourceString[position];

                position++;
                if (currentCharacter == '}')
                {
                    tokenBuffer.Append(currentCharacter);

                    // Treat as escape character for }}
                    while (position < length && resourceString[position] == '}') 
                    {
                        currentCharacter = resourceString[position];
                        tokenBuffer.Append(currentCharacter);
                        position++;
                    }

                    if (tokenBuffer != null && tokenBuffer.Length > 0)
                    {
                        var formattedString = string.Format(tokenBuffer.ToString(), arguments);
                        outputBuffer.Append(EncodeArgument(formattedString));
                    }

                    if (position == length)
                    {
                        break;
                    }
                    else
                    {
                        currentCharacter = resourceString[position];
                        position++;
                        isToken = false;
                    }
                }

                if (currentCharacter == '{')
                {
                    tokenBuffer = new StringBuilder();
                    tokenBuffer.Append(currentCharacter);
                    isToken = true;

                    // Treat as escape character for {{
                    while (position < length && resourceString[position] == '{')
                    {
                        currentCharacter = resourceString[position];

                        tokenBuffer.Append(currentCharacter);
                        position++;
                    }

                    //Remove the last added { since it is getting added in saveData condition below.
                    tokenBuffer.Length -= 1;
                }

                if (isToken)
                {
                    tokenBuffer.Append(currentCharacter);
                }
                else
                {
                    outputBuffer.Append(currentCharacter);
                }
            }

            return outputBuffer.ToString();
        }

        /// <summary>
        /// Encodes the argument.
        /// </summary>
        /// <param name="argument">The object to encode.</param>
        /// <returns>The encoded object.</returns>
        protected object EncodeArgument([NotNull] object argument)
        {
            Debug.Assert(!(argument is HtmlString));

            return _encoder.HtmlEncode(argument.ToString());
        }
    }
}