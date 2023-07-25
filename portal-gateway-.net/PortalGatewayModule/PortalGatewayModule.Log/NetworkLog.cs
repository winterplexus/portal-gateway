//
//  NetworkLog.cs
//
//  Wiregrass Code Technology 2020-2023
//
using System;
using System.Net;
using System.Globalization;
using System.Web;
using NLog;
using PortalGatewayModule.Utility;

namespace PortalGatewayModule.Log
{
    public static class NetworkLog
    {
        private const string singleIndent = "  ·";
        private const string doubleIndent = "    >";
        private const string tripleIndent = "      |";

        public static void WriteRequest(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var logger = LogManager.GetLogger("Network");

            logger.Info(CultureInfo.InvariantCulture, "http request");
            logger.Info(CultureInfo.InvariantCulture, "{0} method: {1}", singleIndent, request.HttpMethod);
            logger.Info(CultureInfo.InvariantCulture, "{0} raw url: {1}", singleIndent, request.RawUrl);
            logger.Info(CultureInfo.InvariantCulture, "{0} request uri", singleIndent);
            logger.Info(CultureInfo.InvariantCulture, "{0} absolute uri: {1}", doubleIndent, request.Url.AbsoluteUri);
            logger.Info(CultureInfo.InvariantCulture, "{0} absolute path: {1}", doubleIndent, request.Url.AbsolutePath);
            logger.Info(CultureInfo.InvariantCulture, "{0} local path: {1}", doubleIndent, request.Url.LocalPath);
            logger.Info(CultureInfo.InvariantCulture, "{0} query: {1}", doubleIndent, request.Url.Query);
            logger.Info(CultureInfo.InvariantCulture, "{0} user agent: {1}", singleIndent, request.UserAgent);

            WriteRequestQueryString(logger, request);
            WriteRequestParameters(logger, request);
            WriteRequestServerVariables(logger, request);
            WriteRequestForm(logger, request);
            WriteRequestHeaders(logger, request);
            WriteRequestCookies(logger, request);
        }

        public static void WriteForwardRequest(HttpWebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var logger = LogManager.GetLogger("Network");

            logger.Info(CultureInfo.InvariantCulture, "http forward request");
            logger.Info(CultureInfo.InvariantCulture, "{0} method: {1}", singleIndent, request.Method);
            logger.Info(CultureInfo.InvariantCulture, "{0} host: {1}", singleIndent, request.Host);
            logger.Info(CultureInfo.InvariantCulture, "{0} address: {1}", singleIndent, request.Address);
            logger.Info(CultureInfo.InvariantCulture, "{0} request uri", singleIndent);
            logger.Info(CultureInfo.InvariantCulture, "{0} absolute uri: {1}", doubleIndent, request.RequestUri.AbsoluteUri);
            logger.Info(CultureInfo.InvariantCulture, "{0} absolute path: {1}", doubleIndent, request.RequestUri.AbsolutePath);
            logger.Info(CultureInfo.InvariantCulture, "{0} local path: {1}", doubleIndent, request.RequestUri.LocalPath);
            logger.Info(CultureInfo.InvariantCulture, "{0} query: {1}", doubleIndent, request.RequestUri.Query);
            logger.Info(CultureInfo.InvariantCulture, "{0} referer: {1}", singleIndent, request.Referer);
            logger.Info(CultureInfo.InvariantCulture, "{0} allow auto redirect: {1}", singleIndent, Assistant.GetBooleanValue(request.AllowAutoRedirect));
            logger.Info(CultureInfo.InvariantCulture, "{0} accept: {1}", singleIndent, request.Accept);
            logger.Info(CultureInfo.InvariantCulture, "{0} timeout: {1}", singleIndent, request.Timeout);
            logger.Info(CultureInfo.InvariantCulture, "{0} continue timeout: {1}", singleIndent, request.ContinueTimeout);
            logger.Info(CultureInfo.InvariantCulture, "{0} keepalive: {1}", singleIndent, Assistant.GetBooleanValue(request.KeepAlive));
            logger.Info(CultureInfo.InvariantCulture, "{0} send chunked: {1}", singleIndent, Assistant.GetBooleanValue(request.SendChunked));
            logger.Info(CultureInfo.InvariantCulture, "{0} user agent: {1}", singleIndent, request.UserAgent);

            WriteForwardRequestHeaders(logger, request);
        }

        public static void WriteForwardResponse(HttpWebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var logger = LogManager.GetLogger("Network");

            logger.Info(CultureInfo.InvariantCulture, "http forward response");
            logger.Info(CultureInfo.InvariantCulture, "{0} method: {1}", singleIndent, response.Method);
            logger.Info(CultureInfo.InvariantCulture, "{0} server: {1}", singleIndent, response.Server);
            logger.Info(CultureInfo.InvariantCulture, "{0} response uri", singleIndent);
            logger.Info(CultureInfo.InvariantCulture, "{0} absolute uri: {1}", doubleIndent, response.ResponseUri.AbsoluteUri);
            logger.Info(CultureInfo.InvariantCulture, "{0} absolute path: {1}", doubleIndent, response.ResponseUri.AbsolutePath);
            logger.Info(CultureInfo.InvariantCulture, "{0} local path: {1}", doubleIndent, response.ResponseUri.LocalPath);
            logger.Info(CultureInfo.InvariantCulture, "{0} query: {1}", doubleIndent, response.ResponseUri.Query);
            logger.Info(CultureInfo.InvariantCulture, "{0} character set: {1}", singleIndent, response.CharacterSet);
            logger.Info(CultureInfo.InvariantCulture, "{0} content length: {1}", singleIndent, response.ContentLength);
            logger.Info(CultureInfo.InvariantCulture, "{0} content type: {1}", singleIndent, response.ContentType);
            logger.Info(CultureInfo.InvariantCulture, "{0} status code: {1}", singleIndent, (int)response.StatusCode);
            logger.Info(CultureInfo.InvariantCulture, "{0} status description: {1}", singleIndent, response.StatusDescription);

            WriteForwardResponseHeaders(logger, response);
        }

        private static void WriteRequestQueryString(ILogger logger, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            logger.Info(CultureInfo.InvariantCulture, "{0} query string:", singleIndent);

            foreach (string queryString in request.QueryString)
            {
                logger.Info(CultureInfo.InvariantCulture, "{0} {1}: {2}", doubleIndent, queryString, ReplaceControlCharacters(request.QueryString[queryString]));
            }
        }

        private static void WriteRequestParameters(ILogger logger, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            logger.Info(CultureInfo.InvariantCulture, "{0} parameters:", singleIndent);

            foreach (string parameter in request.Params)
            {
                logger.Info(CultureInfo.InvariantCulture, "{0} {1}: {2}", doubleIndent, parameter, ReplaceControlCharacters(request.Params[parameter]));
            }
        }

        private static void WriteRequestServerVariables(ILogger logger, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            logger.Info(CultureInfo.InvariantCulture, "{0} server variables:", singleIndent);

            foreach (string serverVariable in request.ServerVariables)
            {
                logger.Info(CultureInfo.InvariantCulture, "{0} {1}: {2}", doubleIndent, serverVariable, ReplaceControlCharacters(request.ServerVariables[serverVariable]));
            }
        }

        private static void WriteRequestForm(ILogger logger, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            logger.Info(CultureInfo.InvariantCulture, "{0} form(s):", singleIndent);

            foreach (string form in request.Form)
            {
                logger.Info(CultureInfo.InvariantCulture, "{0} {1}: {2}", doubleIndent, form, ReplaceControlCharacters(request.Form[form]));
            }
        }

        private static void WriteRequestHeaders(ILogger logger, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            logger.Info(CultureInfo.InvariantCulture, "{0} headers:", singleIndent);

            foreach (string header in request.Headers)
            {
                logger.Info(CultureInfo.InvariantCulture, "{0} {1}: {2}", doubleIndent, header, ReplaceControlCharacters(request.Headers[header]));
            }
        }

        private static void WriteRequestCookies(ILogger logger, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            logger.Info(CultureInfo.InvariantCulture, "{0} cookies:", singleIndent);

            var cookies = request.Cookies;
            for (var i = 0; i < cookies.Count; i++)
            {
                if (cookies[i] == null)
                {
                    continue;
                }
                logger.Info(CultureInfo.InvariantCulture, "{0} cookie {1}", doubleIndent, i);
                logger.Info(CultureInfo.InvariantCulture, "{0} name: {1}", tripleIndent, cookies[i].Name);
                logger.Info(CultureInfo.InvariantCulture, "{0} value: {1}", tripleIndent, cookies[i].Value);
                logger.Info(CultureInfo.InvariantCulture, "{0} expires: {1}", tripleIndent, cookies[i].Expires);
                logger.Info(CultureInfo.InvariantCulture, "{0} path: {1}", tripleIndent, cookies[i].Path);
                logger.Info(CultureInfo.InvariantCulture, "{0} domain: {1}", tripleIndent, cookies[i].Domain);
            }
        }

        private static void WriteForwardRequestHeaders(ILogger logger, HttpWebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            logger.Info(CultureInfo.InvariantCulture, "{0} headers:", singleIndent);

            if (request.Headers == null)
            {
                return;
            }

            foreach (string header in request.Headers)
            {
                logger.Info(CultureInfo.InvariantCulture, "{0} {1}: {2}", doubleIndent, header, ReplaceControlCharacters(request.Headers[header]));
            }
        }

        private static void WriteForwardResponseHeaders(ILogger logger, HttpWebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            logger.Info(CultureInfo.InvariantCulture, "{0} headers:", singleIndent);

            if (response.Headers == null)
            {
                return;
            }

            foreach (string header in response.Headers)
            {
                logger.Info(CultureInfo.InvariantCulture, "{0} {1}: {2}", doubleIndent, header, ReplaceControlCharacters(response.Headers[header]));
            }
        }

        private static string ReplaceControlCharacters(string input)
        {
            return string.IsNullOrEmpty(input) ? input : input.Replace(EscapeCharacters.Bell, "").
                                                               Replace(EscapeCharacters.Backspace, "").
                                                               Replace(EscapeCharacters.FormFeed, " | ").
                                                               Replace(EscapeCharacters.Linefeed, " | ").
                                                               Replace(EscapeCharacters.CarriageReturn, " | ").
                                                               Replace(EscapeCharacters.HorizontalTab, " ").
                                                               Replace(EscapeCharacters.VerticalTab, " | ");
        }
    }
}