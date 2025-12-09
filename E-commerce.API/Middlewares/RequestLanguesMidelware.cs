using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace E_commerce.API.Middlewares
{
    public class RequestLanguesMidelware
    {
        private readonly RequestDelegate _next;

        public RequestLanguesMidelware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var currentLanguage = context.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            var browserLanguageHeader = context.Request.Headers["Accept-Language"].ToString();

            string browserLanguage = string.Empty;

            if (!string.IsNullOrEmpty(browserLanguageHeader) && browserLanguageHeader.Length >= 2)
            {
                browserLanguage = browserLanguageHeader[..2].ToLower();
            }

            if (string.IsNullOrEmpty(currentLanguage))
            {
                var langes = string.Empty;

                if (browserLanguage == "fr")
                {
                    langes = "fr-FR";
                }
                else if (browserLanguage == "ar")
                {
                    langes = "ar-EG";
                }
                else
                {
                    langes = "en-US";
                }

                CultureInfo.CurrentCulture = new CultureInfo(langes);
            }

            await _next(context);
        }
    }
}
