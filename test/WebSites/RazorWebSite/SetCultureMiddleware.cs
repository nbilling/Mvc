
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

namespace RazorWebSite
{
    public class SetCultureMiddleware
    {
        private readonly RequestDelegate _next;

        public SetCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var locValue = httpContext.Request.Headers["loc"];

            if (!string.IsNullOrEmpty(locValue))
            {
                var culture = new CultureInfo(locValue);
#if DNX451
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
#else
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
#endif
            }
            return _next.Invoke(httpContext);
        }
    }
}
