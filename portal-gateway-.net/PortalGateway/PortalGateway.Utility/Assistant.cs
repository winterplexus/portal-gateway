//
//  Assistant.cs
//
//  Wiregrass Code Technology 2020-2023 
//
using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

[assembly: CLSCompliant(true)]
namespace PortalGateway.Utility
{
    public static class Assistant
    {
        public static string GetConfigurationValue(string name)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[name]) ? ConfigurationManager.AppSettings[name] : string.Empty;
        }

        public static string GetBooleanValue(bool value)
        {
            return value ? "true" : "false";
        }

        public static int GetNumberValue(string value)
        {
            return int.TryParse(value, out int number) ? number : 0;
        }

        public static string GetMethodFullName(MethodBase methodInfo)
        {
            if (methodInfo == null)
            {
                return string.Empty;
            }
            return methodInfo.DeclaringType != null ? string.Format(CultureInfo.InvariantCulture, "{0}.{1}()", methodInfo.DeclaringType.FullName, methodInfo.Name) : null;
        }

        public static string GetDomainUserNameOnly(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }
            return Regex.Replace(userName, ".*\\\\(.*)", "$1", RegexOptions.None);
        }
    }
}