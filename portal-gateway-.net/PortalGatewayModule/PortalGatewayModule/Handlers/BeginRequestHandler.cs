//
//  BeginRequestHandler.cs
//
//  Wiregrass Code Technology 2020-2023
//
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Web;
using System.Web.Security;
using PortalGatewayModule.Log;
using PortalGatewayModule.Utility;

namespace PortalGatewayModule
{
    public class BeginRequestHandler
    {
        private readonly HttpApplication application;
        private readonly HttpRequest currentRequest;
        private readonly bool networkLogEnabled;
        private HttpWebRequest forwardRequest;
        private FormsAuthenticationTicket authenticationTicket;
        private DateTime authenticationTicketExpiration;
        private HttpCookie authenticationCookie;

        public BeginRequestHandler(HttpApplication application)
        {
            if (application == null)
            {
                return;
            }

            this.application = application;

            currentRequest = application.Request;

            if (Assistant.GetConfigurationValue("NetworkLogEnabled").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                networkLogEnabled = true;
            }
        }
        public bool ProcessRequest()
        {
            if (application == null)
            {
                return false;
            }
#if _ENABLE_DEBUGGER
            System.Diagnostics.Debugger.Launch();
#endif
            if (networkLogEnabled)
            {
                NetworkLog.WriteRequest(currentRequest);
            }

            if (!AuthenticateUser() || ExcludedPath() || !CreateForwardRequest(GetForwardUrl()))
            {
                return false;
            }

            SetExcludeCookies();
            SetHeaders();
            SendForwardRequest();
            UpdateAuthenticationTicket();

            return true;
        }

        private bool AuthenticateUser()
        {
            authenticationCookie = currentRequest.Cookies[FormsAuthentication.FormsCookieName];
            if (authenticationCookie == null)
            {
                return false;
            }

            authenticationTicket = FormsAuthentication.Decrypt(authenticationCookie.Value);
            if (authenticationTicket == null)
            {
                return false;
            }
            if (authenticationTicket.Expired)
            {
                return false;
            }

            authenticationTicketExpiration = authenticationTicket.Expiration;
            authenticationTicket = FormsAuthentication.RenewTicketIfOld(authenticationTicket);

            return true;
        }

        private bool ExcludedPath()
        {
            if (string.IsNullOrEmpty(Assistant.GetConfigurationValue("ExcludePaths")))
            {
                return false;
            }

            var excludePaths = Assistant.GetConfigurationValue("ExcludePaths").Split(',');

            return excludePaths.Any(path => currentRequest.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase));
        }

        private bool CreateForwardRequest(string forwardUrl)
        {
            forwardRequest = WebRequest.Create(forwardUrl) as HttpWebRequest;
            if (forwardRequest == null)
            {
                return false;
            }

            var httpTimeout = Assistant.GetConfigurationValue("HttpTimeout");
            if (!string.IsNullOrWhiteSpace(httpTimeout))
            {
                forwardRequest.Timeout = Assistant.GetNumberValue(httpTimeout);
            }
            var httpReadWriteTimeout = Assistant.GetConfigurationValue("HttpReadWriteTimeout");
            if (!string.IsNullOrWhiteSpace(httpReadWriteTimeout))
            {
                forwardRequest.ReadWriteTimeout = Assistant.GetNumberValue(httpReadWriteTimeout);
            }
            var forwardCertificateThumbprint = Assistant.GetConfigurationValue("ForwardCertificateThumbprint");
            if (!string.IsNullOrWhiteSpace(forwardCertificateThumbprint))
            {
                forwardRequest.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
                forwardRequest.ClientCertificates.Add(Certificates.GetCertificate(forwardCertificateThumbprint));
            }

            forwardRequest.AllowAutoRedirect = false;
            forwardRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Default);
            forwardRequest.KeepAlive = true;

            return true;
        }

        private string GetForwardUrl()
        {
            var forwardUrl = Assistant.GetConfigurationValue("ForwardUrl");

            if (string.IsNullOrWhiteSpace(currentRequest.RawUrl) || string.Equals(currentRequest.RawUrl, "/"))
            {
                forwardUrl = Urls.Combine(forwardUrl, Assistant.GetConfigurationValue("ForwardStartupPath"));
            }
            else
            {
                forwardUrl = Urls.Combine(forwardUrl, currentRequest.RawUrl);
            }

            return forwardUrl;
        }

        private void SetExcludeCookies()
        {
            string[] excludeCookieNames = null;

            if (!string.IsNullOrEmpty(Assistant.GetConfigurationValue("ExcludeCookies")))
            {
                excludeCookieNames = Assistant.GetConfigurationValue("ExcludeCookies").Split(',');
            }

            currentRequest.Copy(forwardRequest, excludeCookieNames);
        }

        private void SetHeaders()
        {
            var localIP = NetworkAddress.LocalIP();

            if (string.IsNullOrWhiteSpace(currentRequest.Headers["X-Forwarded-For"]))
            {
                forwardRequest.Headers["X-Forwarded-For"] = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", currentRequest.UserHostAddress, localIP);
            }
            else
            {
                forwardRequest.Headers["X-Forwarded-For"] = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", forwardRequest.Headers["X-Forwarded-For"], localIP);
            }

            forwardRequest.Headers["X-Forwarded-Host"] = currentRequest.Url.Host;
            forwardRequest.Headers["UserId"] = authenticationTicket.Name;
            forwardRequest.Headers["UserRole"] = authenticationTicket.UserData;
            forwardRequest.Headers["RequestServer"] = localIP;
            forwardRequest.Headers["CompanyName"] = Assistant.GetConfigurationValue("CompanyName");
            forwardRequest.Headers["CompanyRequestor"] = Assistant.GetConfigurationValue("CompanyRequestor");
            forwardRequest.Headers["CompanyGeneratedIdentifier"] = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

            var privateMethod = forwardRequest.Headers.GetType().GetMethod("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
            privateMethod.Invoke(forwardRequest.Headers, new object[] { "Date", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture) });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "filter")]
        private void SendForwardRequest()
        {
            HttpWebResponse forwardResponse = null;

            if (networkLogEnabled)
            {
                NetworkLog.WriteForwardRequest(forwardRequest);
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            forwardRequest.ServicePoint.Expect100Continue = false;

            try
            {
                forwardResponse = forwardRequest.GetResponse() as HttpWebResponse;
            }
            catch (WebException we)
            {
                forwardResponse = we.Response as HttpWebResponse;

                var message = string.Format(CultureInfo.InvariantCulture, "web exception for forward request absolute uri {0}", forwardRequest.RequestUri.AbsoluteUri);

                EventsLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), message, we);
            }
            finally
            {
                if (forwardResponse != null)
                {
                    if (forwardResponse.ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        // Filter object must be referenced.
                        var filter = application.Response.Filter;
                        application.Response.Filter = new HttpResponseFilterStream(application.Response.OutputStream);
                    }

                    forwardResponse.Copy(application.Response);

                    if (networkLogEnabled)
                    {
                        NetworkLog.WriteForwardResponse(forwardResponse);
                    }

                    forwardResponse.Dispose();
                }
            }
        }

        private void UpdateAuthenticationTicket()
        {
            if (authenticationTicketExpiration != authenticationTicket.Expiration)
            {
                return;
            }

            var encryptedFormsAuthenticationTicket = FormsAuthentication.Encrypt(authenticationTicket);
            authenticationCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedFormsAuthenticationTicket);
            application.Response.Cookies.Add(authenticationCookie);
        }
    }
}