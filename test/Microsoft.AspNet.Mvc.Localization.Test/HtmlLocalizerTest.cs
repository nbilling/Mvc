// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Localization;
using Microsoft.Framework.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Localization.Test
{
    public class HtmlLocalizerTest
    {
        [Fact]
        public void HtmlLocalizer_GetLocalizedString()
        {
            // Arrange
            var localizedString = new LocalizedString("Hello", "Bonjour");
            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s["Hello"]).Returns(localizedString);

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer.Object, new CommonTestEncoder());

            // Act
            var actualLocalizedString = htmlLocalizer["Hello"];

            // Assert
            Assert.Equal(localizedString, actualLocalizedString);
        }

        [Fact]
        public void HtmlLocalizer_GetLocalizedStringWithArguments()
        {
            // Arrange
            var localizedString = new LocalizedString("Hello", "Bonjour test");

            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s["Hello", "test"]).Returns(localizedString);

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer.Object, new CommonTestEncoder());

            // Act
            var actualLocalizedString = htmlLocalizer["Hello", "test"];

            // Assert
            Assert.Equal(localizedString, actualLocalizedString);
        }

        [Fact]
        public void HtmlLocalizerOfT_GetLocalizedString()
        {
            // Arrange
            var localizedString = new LocalizedString("Hello", "Bonjour");

            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s["Hello"]).Returns(localizedString);

            var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
            stringLocalizerFactory.Setup(s => s.Create(typeof(TestClass))).Returns(stringLocalizer.Object);

            var actualHtmlLocalizer = new HtmlLocalizer<TestClass>(
                stringLocalizerFactory.Object,
                new CommonTestEncoder());

            // Act
            var actualLocalizedString = actualHtmlLocalizer["Hello"];

            // Assert
            Assert.Equal(localizedString, actualLocalizedString);
        }

        [Fact]
        public void HtmlLocalizerOfT_GetLocalizedStringWithArguments()
        {
            // Arrange
            var localizedString = new LocalizedString("Hello", "Bonjour test");

            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s["Hello", "test"]).Returns(localizedString);

            var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
            stringLocalizerFactory.Setup(s => s.Create(typeof(TestClass))).Returns(stringLocalizer.Object);

            var actualHtmlLocalizer = new HtmlLocalizer<TestClass>(
                stringLocalizerFactory.Object,
                new CommonTestEncoder());

            // Act
            var actualLocalizedString = actualHtmlLocalizer["Hello", "test"];

            // Assert
            Assert.Equal(localizedString, actualLocalizedString);
        }
    }

    public class TestClass
    {

    }
}
