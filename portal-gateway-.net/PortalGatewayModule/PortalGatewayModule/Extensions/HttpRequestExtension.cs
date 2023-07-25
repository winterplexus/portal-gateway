//
//  HttpRequestExtension.cs
//
//  Wiregrass Code Technology 2020-2023
//
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace PortalGatewayModule
{
    public static class HttpRequestExtension
    {
        public static void Copy(this HttpRequest source, HttpWebRequest destination, string[] excludeCookieNames)
        {
            destination.Method = source.HttpMethod;

            foreach (var headerKey in source.Headers.AllKeys)
            {
                if (CopiedByServer(headerKey) || CopiedByExtension(headerKey))
                {
                }
                else if (headerKey == "Cookie")
                {
                    CopyCookieHeader(source, destination, excludeCookieNames, headerKey);
                }
                else
                {
                    destination.Headers[headerKey] = source.Headers[headerKey];
                }
            }

            if (source.AcceptTypes != null && source.AcceptTypes.Any())
            {
                destination.Accept = string.Join(",", source.AcceptTypes);
            }

            destination.ContentType = source.ContentType;
            destination.Referer = source.Url.AbsoluteUri;
            destination.UserAgent = source.UserAgent;

            if (source.ContentLength <= 0 || (source.HttpMethod == "GET" || source.HttpMethod == "HEAD"))
            {
                return;
            }

            using (var requestStream = destination.GetRequestStream())
            {
                source.InputStream.CopyTo(requestStream);
            }
        }

        private static bool CopiedByServer(string headerKey)
        {
            return (headerKey == "Connection" ||
                    headerKey == "Content-Length" ||
                    headerKey == "Date" ||
                    headerKey == "Expect" ||
                    headerKey == "Host" ||
                    headerKey == "If-Modified-Since" ||
                    headerKey == "Range" ||
                    headerKey == "Transfer-Encoding" ||
                    headerKey == "Proxy-Connection");
        }

        private static bool CopiedByExtension(string headerKey)
        {
            return (headerKey == "Accept" ||
                    headerKey == "Content-Type" ||
                    headerKey == "Referer" ||
                    headerKey == "User-Agent");
        }

        private static void CopyCookieHeader(HttpRequest source, HttpWebRequest destination, string[] excludedCookieNames, string headerKey)
        {
            if (excludedCookieNames != null && excludedCookieNames.Any())
            {
                var destinationCookieHeader = new StringBuilder();
                var cookiesInSourceHeader = source.Headers[headerKey].Split(';');
                var counter = 0;

                foreach (var cookie in from cookie in cookiesInSourceHeader let cookieName = cookie.Substring(0, cookie.IndexOf("=", System.StringComparison.Ordinal)).Trim() where !excludedCookieNames.Contains(cookieName) select cookie)
                {
                    counter++;
                    if (counter > 1)
                    {
                        destinationCookieHeader.Append("; ");
                    }
                    destinationCookieHeader.Append(cookie.Trim());
                }
                destination.Headers[headerKey] = destinationCookieHeader.ToString();
            }
            else
            {
                destination.Headers[headerKey] = source.Headers[headerKey];
            }
        }
    }
}