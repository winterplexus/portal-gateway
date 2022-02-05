//
//  Certificates.cs
//
//  Wiregrass Code Technology 2020-2022
//
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace PortalGatewayModule.Utility
{
    public static class Certificates
    {
        public static X509Certificate2 GetCertificate(string thumbprint)
        {
            X509Certificate2 certificate = null;

            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);
                if (certificates.Count == 0)
                {
                    WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), GetErrorMessage(thumbprint));
                }
                else
                {
                    certificate = certificates[0];
                }
            }
            finally
            {
                store.Close();
            }

            return certificate;
        }

        private static string GetErrorMessage(string thumbprint)
        {
            return "Certificate does not exist in local machine's (Personal) certificate store with thumbprint: " + thumbprint;
        }
    }
}