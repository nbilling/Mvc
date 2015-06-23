// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Localization;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Localization.Test
{
    public class ViewLocalizerTest
    {
        [Fact]
        public void ViewLocalizer_GetLocalizedString()
        {
            // Arrange
            var applicationEnvironment = new Mock<IApplicationEnvironment>();
            applicationEnvironment.Setup(a => a.ApplicationName).Returns("TestApplication");

            var localizedString = new LocalizedString("Hello", "Bonjour");

            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s["Hello"]).Returns(localizedString);

            var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
            stringLocalizerFactory.Setup(s => s.Create("TestApplication.example", "TestApplication"))
                .Returns(stringLocalizer.Object);

            var viewLocalizer = new ViewLocalizer(
                stringLocalizerFactory.Object,
                new CommonTestEncoder(),
                applicationEnvironment.Object);

            var view = new Mock<IView>();
            view.Setup(v => v.Path).Returns("example");
            var viewContext = new ViewContext();
            viewContext.View = view.Object;

            viewLocalizer.Contextualize(viewContext);

            // Act
            var actualLocalizedString = viewLocalizer["Hello"];

            // Assert
            Assert.Equal(localizedString, actualLocalizedString);
        }

        [Fact]
        public void ViewLocalizer_GetLocalizedStringWithArguments()
        {
            // Arrange
            var applicationEnvironment = new Mock<IApplicationEnvironment>();
            applicationEnvironment.Setup(a => a.ApplicationName).Returns("TestApplication");

            var localizedString = new LocalizedString("Hello", "Bonjour test");

            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s["Hello", "test"]).Returns(localizedString);
            var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();

            stringLocalizerFactory.Setup(s => s.Create("TestApplication.example", "TestApplication"))
                .Returns(stringLocalizer.Object);

            var viewLocalizer = new ViewLocalizer(
                stringLocalizerFactory.Object,
                new CommonTestEncoder(),
                applicationEnvironment.Object);

            var view = new Mock<IView>();
            view.Setup(v => v.Path).Returns("example");
            var viewContext = new ViewContext();
            viewContext.View = view.Object;

            viewLocalizer.Contextualize(viewContext);

            // Act
            var actualLocalizedString = viewLocalizer["Hello", "test"];

            // Assert
            Assert.Equal(localizedString, actualLocalizedString);
        }
    }
}
