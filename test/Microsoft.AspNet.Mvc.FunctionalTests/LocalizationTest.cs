// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Xml.Linq;
using LocalizationWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class LocalizationTest
    {
        private const string SiteName = nameof(LocalizationWebSite);
        private static readonly Assembly _assembly = typeof(LocalizationTest).GetTypeInfo().Assembly;

        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new Startup().ConfigureServices;

        public static IEnumerable<object[]> LocalizationData
        {
            get
            {
                var expected1 =
 @"<language-layout>
en-gb-index
partial
mypartial
</language-layout>";

                yield return new[] { "en-GB", expected1 };

                var expected2 =
 @"<fr-language-layout>
fr-index
fr-partial
mypartial
</fr-language-layout>";
                yield return new[] { "fr", expected2 };

                var expected3 =
 @"<language-layout>
index
partial
mypartial
</language-layout>";
                yield return new[] { "na", expected3 };

            }
        }

        [Theory]
        [MemberData(nameof(LocalizationData))]
        public async Task Localization_SuffixViewName(string value, string expected)
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var cultureCookie = "c=" + value + "|uic=" + value;
            client.DefaultRequestHeaders.Add(
                "Cookie",
                new CookieHeaderValue("ASPNET_CULTURE", cultureCookie).ToString());

            // Act
            var body = await client.GetStringAsync("http://localhost/");

            // Assert
            Assert.Equal(expected, body.Trim(), ignoreLineEndingDifferences: true);
        }

        public static IEnumerable<object[]> LocalizationResourceData
        {
            get
            {
                var expected1 =
 @"My ASP.NET Application

Hello there!!
Learn More";

                yield return new[] {"en-GB", expected1 };

                var expected2 =
 @"Mon application ASP.NET

Bonjour!
apprendre Encore Plus";

//Coreclr just reads enu resources. Hence the conditional if
#if DNX451
                yield return new[] { "fr", expected2 };
#else
                yield return new[] {"fr", expected1 };
#endif

            }
        }

        [Theory]
        [MemberData(nameof(LocalizationResourceData))]
        public async Task Localization_Resources(string value, string expected)
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var cultureCookie = "c=" + value + "|uic=" + value;
            client.DefaultRequestHeaders.Add(
                "Cookie",
                new CookieHeaderValue("ASPNET_CULTURE", cultureCookie).ToString());

            if (!value.StartsWith("en"))
            {
                // Manually generating .resources file since we don't autogenerate .resources file yet. 
                WriteResourceFile("HomeController." + value + ".resx");
                WriteResourceFile("Views.Home.Locpage.cshtml." + value + ".resx");
                WriteResourceFile("Views.Shared._LocalizationLayout.cshtml." + value + ".resx");
            }

            // Act
            var body = await client.GetStringAsync("http://localhost/Home/Locpage");

            // Assert
            Assert.Equal(expected, body.Trim());
        }

        private void WriteResourceFile(string resxFileName)
        {
            var resxFilePath = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\WebSites\\" + SiteName + 
                "\\Resources\\" + resxFileName;

            if (File.Exists(resxFilePath))
            {
                using (var fs = File.OpenRead(resxFilePath))
                {
                    var document = XDocument.Load(fs);

                    var binDirPath = Path.Combine(Path.GetDirectoryName(resxFilePath), "bin");
                    if (!Directory.Exists(binDirPath))
                    {
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(resxFilePath), "bin"));
                    }

                    // Put in "bin" sub-folder of resx file
                    var targetPath = Path.Combine(
                        binDirPath,
                        Path.ChangeExtension(Path.GetFileName(resxFilePath), ".resources"));

                    using (var targetStream = File.Create(targetPath))
                    {
                        var rw = new ResourceWriter(targetStream);

                        foreach (var e in document.Root.Elements("data"))
                        {
                            var name = e.Attribute("name").Value;
                            var value = e.Element("value").Value;

                            rw.AddResource(name, value);
                        }

                        rw.Generate();
                    }
                }
            }
        }
    }
}
