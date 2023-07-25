//
//  HttpWebResponseExtension.cs
//

//  Wiregrass Code Technology 2020-2023
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using PortalGatewayModule.Utility;

namespace PortalGatewayModule
{
    public static class HttpWebResponseExtension
    {
        public static void Copy(this HttpWebResponse source, HttpResponse destination)
        {
            destination.StatusCode = (int)source.StatusCode;
            destination.ContentType = source.ContentType;

            foreach (var headerKey in source.Headers.AllKeys.Where(headerKey => !headerKey.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)))
            {
                if (headerKey.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                {
                    CookieFromHeaderToHttpResponse(destination, source.Headers[headerKey]);
                }
                else
                {
                    destination.AddHeader(headerKey, source.Headers[headerKey]);
                }
            }

            if (source.StatusCode == HttpStatusCode.MovedPermanently)
            {
                destination.RedirectLocation = source.GetResponseHeader("Location").Replace(source.ResponseUri.GetLeftPart(UriPartial.Authority), string.Empty);
            }
            if (source.StatusCode == HttpStatusCode.Redirect)
            {
                destination.RedirectLocation = source.GetResponseHeader("Location").Replace(source.ResponseUri.GetLeftPart(UriPartial.Authority), string.Empty);
            }

            using (var responseStream = source.GetResponseStream())
            {
                responseStream?.CopyTo(destination.OutputStream);
            }
        }

        private static void CookieFromHeaderToHttpResponse(HttpResponse destination, string cookieHeader)
        {
            var cookieArrayList = CookieHeaderToList(cookieHeader);
            CookieListToHttpCookieCollection(cookieArrayList, destination.Cookies);
        }

        private static IEnumerable<string> CookieHeaderToList(string cookieHeader)
        {
            cookieHeader = cookieHeader.Replace(EscapeCharacters.CarriageReturn, "");
            cookieHeader = cookieHeader.Replace(EscapeCharacters.Linefeed, "");

            var cookiesInHeader = cookieHeader.Split(',');
            var cookiesInHeaderCount = cookiesInHeader.Length;
            var cookieList = new List<string>();

            var i = 0;
            while (i < cookiesInHeaderCount)
            {
                if (cookiesInHeader[i].IndexOf("Expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    cookieList.Add(cookiesInHeader[i] + "," + cookiesInHeader[i + 1]);
                    i++;
                }
                else
                {
                    cookieList.Add(cookiesInHeader[i]);
                }
                i++;
            }

            return cookieList;
        }

        private static void CookieListToHttpCookieCollection(IEnumerable<string> cookieList, HttpCookieCollection httpCookieCollection)
        {
            foreach (var cookie in cookieList)
            {
                var cookieParts = cookie.Split(';');
                var cookiePartsCount = cookieParts.Length;
                var httpCookie = new HttpCookie(string.Empty);

                for (var i = 0; i < cookiePartsCount; i++)
                {
                    if (i == 0)
                    {
                        var cookieNameValue = cookieParts[i];
                        if (!string.IsNullOrEmpty(cookieNameValue))
                        {
                            var equalSign = cookieNameValue.IndexOf("=", StringComparison.Ordinal);
                            httpCookie.Name = cookieNameValue.Substring(0, equalSign);
                            httpCookie.Value = cookieNameValue.Substring(equalSign + 1);
                        }
                        continue;
                    }
                    if (cookieParts[i].IndexOf("Expires", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var expiresNameValuePair = cookieParts[i].Split('=');
                        if (!string.IsNullOrEmpty(expiresNameValuePair[1]))
                        {
                            httpCookie.Expires = DateTime.Parse(expiresNameValuePair[1], CultureInfo.InvariantCulture);
                        }
                        continue;
                    }
                    if (cookieParts[i].IndexOf("HttpOnly", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        httpCookie.HttpOnly = true;
                        continue;
                    }
                    if (cookieParts[i].IndexOf("Path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var nameValuePair = cookieParts[i].Split('=');
                        if (!string.IsNullOrEmpty(nameValuePair[1]))
                        {
                            httpCookie.Path = nameValuePair[1];
                        }
                        continue;
                    }
                    if (cookieParts[i].IndexOf("Secure", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        httpCookie.Secure = true;
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(httpCookie.Path))
                {
                    httpCookie.Path = "/";
                }

                httpCookieCollection.Add(httpCookie);
            }
        }
    }
}