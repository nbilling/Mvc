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
            var ch = '\x0';
            StringBuilder builder = null;
            var resourceStringBuilder = new StringBuilder();

            while (true)
            {
                builder = new StringBuilder();

                var isToken = false;
                var p = position;
                var i = position;

                while (position < length)
                {
                    ch = resourceString[position];

                    position++;
                    if (ch == '}')
                    {
                        if (position < length && resourceString[position] == '}') // Treat as escape character for }}
                        {
                            position++;
                            if (position < length && resourceString[position] != '{')
                            {
                                isToken = false;
                                builder.Append(ch);
                                resourceStringBuilder.Append(EncodeArgument(builder.ToString()));
                                builder = new StringBuilder();
                            }
                        }
                        else
                        {
                            FormatError();
                        }
                    }

                    if (ch == '{')
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder();
                        }
                        if (position < length && resourceString[position] == '{') // Treat as escape character for {{
                        {
                            isToken = true;
                            position++;
                        }
                        else
                        {
                            position--;
                            break;
                        }
                    }

                    if (isToken)
                    {
                        builder.Append(ch);
                    }
                    else
                    {
                        resourceStringBuilder.Append(ch);
                    }
                }

                if (position == length)
                {
                    if (builder.Length > 0)
                    {
                        resourceStringBuilder.Append(EncodeArgument(builder.ToString()));
                    }
                    break;
                }
                position++;
                if (position == length || (ch = resourceString[position]) < '0' || ch > '9')
                {
                    FormatError();
                }

                var index = 0;
                do
                {
                    index = index * 10 + ch - '0';
                    position++;
                    if (position == length)
                    {
                        FormatError();
                    }

                    ch = resourceString[position];
                } while (ch >= '0' && ch <= '9' && index < 1000000);

                if (index >= arguments.Length)
                {
                    FormatError();
                }

                while (position < length && (ch = resourceString[position]) == ' ')
                {
                    position++;
                }

                var leftJustify = false;
                var width = 0;

                if (ch == ',')
                {
                    position++;
                    while (position < length && resourceString[position] == ' ')
                    {
                        position++;
                    }

                    if (position == length)
                    {
                        FormatError();
                    }
                    ch = resourceString[position];
                    if (ch == '-')
                    {
                        leftJustify = true;
                        position++;
                        if (position == length)
                        {
                            FormatError();
                        }
                        ch = resourceString[position];
                    }
                    if (ch < '0' || ch > '9')
                    {
                        FormatError();
                    }
                    do
                    {
                        width = width * 10 + ch - '0';
                        position++;
                        if (position == length)
                        {
                            FormatError();
                        }

                        ch = resourceString[position];
                    } while (ch >= '0' && ch <= '9' && width < 1000000);
                }

                while (position < length && (ch = resourceString[position]) == ' ')
                {
                    position++;
                }

                var arg = arguments[index];
                StringBuilder fmt = null;

                if (ch == ':')
                {
                    position++;
                    p = position;
                    i = position;
                    while (true)
                    {
                        if (position == length)
                        {
                            FormatError();
                        }
                        ch = resourceString[position];
                        position++;

                        if (ch == '{')
                        {
                            // Treat as escape character for {{
                            if (position < length && resourceString[position] == '{')
                            {
                                position++;
                            }
                            else
                            {
                                FormatError();
                            }
                        }
                        else if (ch == '}')
                        {
                            // Treat as escape character for }}
                            if (position < length && resourceString[position] == '}')
                            {
                                position++;
                            }
                            else
                            {
                                position--;
                                break;
                            }
                        }

                        if (fmt == null)
                        {
                            fmt = new StringBuilder();
                        }
                        fmt.Append(ch);
                    }
                }
                if (ch != '}')
                {
                    FormatError();
                }

                string sFmt = null;
                string s = null;

                if (s == null)
                {
                    var formattableArg = arg as IFormattable;

                    if (formattableArg != null)
                    {
                        if (sFmt == null && fmt != null)
                        {
                            sFmt = fmt.ToString();
                        }

                        s = formattableArg.ToString(sFmt, null);
                    }
                    else if (arg != null)
                    {
                        s = arg.ToString();
                    }
                }

                if (s == null)
                {
                    s = string.Empty;
                }
                int pad = width - s.Length;
                if (!leftJustify && pad > 0)
                {
                    builder.Append(' ', pad);
                }

                builder.Append(s);

                if (leftJustify && pad > 0)
                {
                    builder.Append(' ', pad);
                }

                while (true)
                {
                    if (position == length)
                    {
                        FormatError();
                    }
                    ch = resourceString[position];
                    position++;
                    if (ch == '}')
                    {
                        if (position < length && resourceString[position] == '}')  // Treat as escape character for }}
                        {
                            builder.Append(ch);
                            position++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                resourceStringBuilder.Append(EncodeArgument(builder.ToString()));
            }

            return resourceStringBuilder.ToString();
        }

        /// <summary>
        /// Encodes the argument based on the object type.
        /// </summary>
        /// <param name="argument">The object to encode.</param>
        /// <returns>The encoded object.</returns>
        protected object EncodeArgument(object argument)
        {
            if (argument is HtmlString || argument == null)
            {
                return argument;
            }

            return _encoder.HtmlEncode(argument.ToString());
        }

        private void FormatError()
        {
            throw new FormatException(Resources.InvalidResourceString);
        }
    }
}