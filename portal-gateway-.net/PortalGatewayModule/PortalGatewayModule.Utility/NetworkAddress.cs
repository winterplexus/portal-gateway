//
//  NetworkAddress.cs
//
//  Wiregrass Code Technology 2020-2022
//
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace PortalGatewayModule.Utility
{
    public static class NetworkAddress
    {
        public static string LocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            return host == null ? null : (from ip in host.AddressList where ip.AddressFamily == AddressFamily.InterNetwork select ip.ToString()).FirstOrDefault();
        }
    }
}