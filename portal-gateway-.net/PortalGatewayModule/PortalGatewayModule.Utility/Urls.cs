//
//  Urls.cs
//
//  Wiregrass Code Technology 2020-2023
//
using System.Globalization;

namespace PortalGatewayModule.Utility
{
    public static class Urls
    {
        private const char backSlashCharacter = '\\';
        private const char forwardSlashCharacter = '/';

        public static string Combine(params string[] parts)
        {
            var url = string.Empty;

            if (parts == null || parts.Length <= 0)
            {
                return url;
            }

            var trims = new[] { backSlashCharacter, forwardSlashCharacter };

            url = (parts[0] ?? string.Empty).TrimEnd(trims);
            for (var i = 1; i < parts.Length; i++)
            {
                url = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", url.TrimEnd(trims), (parts[i] ?? string.Empty).TrimStart(trims));
            }

            return url;
        }
    }
}