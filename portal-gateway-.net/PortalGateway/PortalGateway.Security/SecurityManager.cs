//
//  SecurityManager.cs
//
//  Wiregrass Code Technology 2020-2022 
//
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using PortalGateway.Utility;

[assembly: CLSCompliant(true)]
namespace PortalGateway.Security
{
    public static class SecurityManager
    {
        private const string domainProtocol = "LDAP://";

        public static bool Authenticated(string userId, string password)
        {
            var domainPaths = GetDomainPaths();

            return domainPaths.Any(domainPath => Bind(domainPath, Assistant.GetDomainUserNameOnly(userId), password));
        }

        public static string UserRoles(string userId)
        {
            return string.IsNullOrEmpty(userId) ? null : GetUserRoles(userId);
        }

        private static bool Bind(string domainPath, string userId, string password)
        {
            var domain = domainProtocol + domainPath;

            try
            {
                using (var entry = new DirectoryEntry(domain, Assistant.GetDomainUserNameOnly(userId), password, AuthenticationTypes.Secure))
                {
                    if (!string.IsNullOrEmpty(entry.NativeGuid))
                    {
                        return true;
                    }
                }
            }
            catch (DirectoryServicesCOMException)
            {
                // ignore invalid user ID or password
            }
            catch (COMException)
            {
                // ignore invalid user ID or password
            }
            return false;
        }

        private static IEnumerable<string> GetDomainPaths()
        {
            var domainPathsConfiguration = Assistant.GetConfigurationValue("DomainPaths");
            if (domainPathsConfiguration == null)
            {
                return null;
            }

            var enumerator = new CommaSeparatedValues().Parse(domainPathsConfiguration);

            var domainPaths = new List<string>();

            try
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != null)
                    {
                        var path = (string)enumerator.Current;
                        if (path.Trim().StartsWith(domainProtocol, StringComparison.OrdinalIgnoreCase))
                        {
                            var lastLocation = path.IndexOf(domainProtocol, StringComparison.OrdinalIgnoreCase);
                            if (lastLocation >= 0)
                            {
                                path = path.Substring(lastLocation + domainProtocol.Length);
                            }
                        }
                        domainPaths.Add(path);
                    }
                }
            }
            catch (InvalidOperationException ioe)
            {
                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "invalid operation exception occurred while parsing domain paths configuration value: " + domainPathsConfiguration, ioe);
            }

            return domainPaths;
        }

        private static string GetUserRoles(string userId)
        {
            string userRoles = null;

            try
            {
                var restRequest = Assistant.GetConfigurationValue("UserRolesServerUrl") + "/?UserId=" + userId;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(restRequest));
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded; encoding='utf-8'";

                var response = (HttpWebResponse)request.GetResponse();

                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var restResponse = new byte[response.ContentLength];
                    responseStream.Read(restResponse, 0, (int)response.ContentLength);

                    var encoding = new ASCIIEncoding();
                    userRoles = encoding.GetString(restResponse).Replace("\"", "");
                    if (string.IsNullOrEmpty(userRoles))
                    {
                        return "user ID has no user roles";
                    }
                }
            }
            catch (WebException we)
            {
                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "web exception occurred while retrieving user roles for user ID: " + userId, we);
            }
            catch (IOException ioe)
            {
                WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "I/O exception occurred while retrieving user roles for user ID: " + userId, ioe);
            }

            return userRoles;
        }
    }
}