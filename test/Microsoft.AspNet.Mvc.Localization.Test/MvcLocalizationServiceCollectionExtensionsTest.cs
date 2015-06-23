// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Localization;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;
using Microsoft.Framework.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Localization.Test
{
    public class MvcLocalizationServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddMvcLocalization_AddsNeededServices()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            MvcLocalizationServiceCollectionExtensions.AddMvcLocalization(collection);

            // Assert
            var services = collection.ToList();
            Assert.Equal(6, services.Count);

            Assert.Equal(typeof(IConfigureOptions<RazorViewEngineOptions>), services[0].ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, services[0].Lifetime);

            Assert.Equal(typeof(IHtmlLocalizer<>), services[1].ServiceType);
            Assert.Equal(typeof(HtmlLocalizer<>), services[1].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[1].Lifetime);

            Assert.Equal(typeof(IHtmlLocalizer), services[2].ServiceType);
            Assert.Equal(typeof(ViewLocalizer), services[2].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[2].Lifetime);

            Assert.Equal(typeof(IHtmlEncoder), services[3].ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, services[3].Lifetime);

            Assert.Equal(typeof(IStringLocalizerFactory), services[4].ServiceType);
            Assert.Equal(typeof(ResourceManagerStringLocalizerFactory), services[4].ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, services[4].Lifetime);

            Assert.Equal(typeof(IStringLocalizer<>), services[5].ServiceType);
            Assert.Equal(typeof(StringLocalizer<>), services[5].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[5].Lifetime);
        }

        [Fact]
        public void AddCustomLocalizers_BeforeMvcLocalization()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            collection.Add(ServiceDescriptor.Transient(typeof(IHtmlLocalizer<>), typeof(TestHtmlLocalizer<>)));
            collection.Add(ServiceDescriptor.Transient(typeof(IHtmlLocalizer), typeof(TestViewLocalizer)));
            collection.Add(ServiceDescriptor.Instance(typeof(IHtmlEncoder), typeof(CommonTestEncoder)));

            MvcLocalizationServiceCollectionExtensions.AddMvcLocalization(collection);

            // Assert
            var services = collection.ToList();
            Assert.Equal(6, services.Count);

            Assert.Equal(typeof(IHtmlLocalizer<>), services[0].ServiceType);
            Assert.Equal(typeof(TestHtmlLocalizer<>), services[0].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[0].Lifetime);

            Assert.Equal(typeof(IHtmlLocalizer), services[1].ServiceType);
            Assert.Equal(typeof(TestViewLocalizer), services[1].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[1].Lifetime);

            Assert.Equal(typeof(IHtmlEncoder), services[2].ServiceType);
            Assert.Equal(typeof(CommonTestEncoder), services[2].ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, services[2].Lifetime);
        }

        [Fact]
        public void AddCustomLocalizers_AfterMvcLocalization()
        {
            // Arrange
            var collection = new ServiceCollection();

            collection.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new CustomPartialDirectoryViewLocationExpander());
            });

            // Act
            MvcLocalizationServiceCollectionExtensions.AddMvcLocalization(collection);

            collection.Add(ServiceDescriptor.Transient(typeof(IHtmlLocalizer<>), typeof(TestHtmlLocalizer<>)));
            collection.Add(ServiceDescriptor.Transient(typeof(IHtmlLocalizer), typeof(TestViewLocalizer)));
            collection.Add(ServiceDescriptor.Instance(typeof(IHtmlEncoder), typeof(CommonTestEncoder)));

            // Assert
            var services = collection.ToList();
            Assert.Equal(10, services.Count);

            Assert.Equal(typeof(IConfigureOptions<RazorViewEngineOptions>), services[0].ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, services[0].Lifetime);
            Assert.Equal(0, ((IConfigureOptions<RazorViewEngineOptions>)services[0].ImplementationInstance).Order);

            Assert.Equal(typeof(IConfigureOptions<RazorViewEngineOptions>), services[1].ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, services[1].Lifetime);
            Assert.Equal(-1000, ((IConfigureOptions<RazorViewEngineOptions>)services[1].ImplementationInstance).Order);

            Assert.Equal(typeof(IHtmlLocalizer<>), services[2].ServiceType);
            Assert.Equal(typeof(HtmlLocalizer<>), services[2].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[2].Lifetime);

            Assert.Equal(typeof(IHtmlLocalizer), services[3].ServiceType);
            Assert.Equal(typeof(ViewLocalizer), services[3].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[3].Lifetime);

            Assert.Equal(typeof(IHtmlEncoder), services[4].ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, services[4].Lifetime);

            Assert.Equal(typeof(IStringLocalizerFactory), services[5].ServiceType);
            Assert.Equal(typeof(ResourceManagerStringLocalizerFactory), services[5].ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, services[5].Lifetime);

            Assert.Equal(typeof(IStringLocalizer<>), services[6].ServiceType);
            Assert.Equal(typeof(StringLocalizer<>), services[6].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[6].Lifetime);

            Assert.Equal(typeof(IHtmlLocalizer<>), services[7].ServiceType);
            Assert.Equal(typeof(TestHtmlLocalizer<>), services[7].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[7].Lifetime);

            Assert.Equal(typeof(IHtmlLocalizer), services[8].ServiceType);
            Assert.Equal(typeof(TestViewLocalizer), services[8].ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, services[8].Lifetime);

            Assert.Equal(typeof(IHtmlEncoder), services[9].ServiceType);
            Assert.Equal(typeof(CommonTestEncoder), services[9].ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, services[9].Lifetime);
        }
    }

    public class TestViewLocalizer : IHtmlLocalizer
    {
        public LocalizedString this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
        {
            throw new NotImplementedException();
        }

        public LocalizedHtmlString Html(string key)
        {
            throw new NotImplementedException();
        }

        public LocalizedHtmlString Html(string key, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        IStringLocalizer IStringLocalizer.WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TestHtmlLocalizer<HomeController> : IHtmlLocalizer<HomeController>
    {
        public LocalizedString this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
        {
            throw new NotImplementedException();
        }

        public LocalizedHtmlString Html(string key)
        {
            throw new NotImplementedException();
        }

        public LocalizedHtmlString Html(string key, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        IStringLocalizer IStringLocalizer.WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomPartialDirectoryViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            throw new NotImplementedException();
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
