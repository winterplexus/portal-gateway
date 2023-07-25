//
//  HttpModule.cs
//
//  Wiregrass Code Technology 2020-2023
//
using System;
using System.Web;

[assembly: CLSCompliant(true)]
namespace PortalGatewayModule
{
    public class HttpModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            if (context != null)
            {
                context.BeginRequest += (OnBeginRequest);
            }
        }

        public void OnBeginRequest(object source, EventArgs args)
        {
            var application = source as HttpApplication;
            if (application == null)
            {
                return;
            }

            var handler = new BeginRequestHandler(application);
            if (!handler.ProcessRequest())
            {
                return;
            }

            application.CompleteRequest();
        }
    }
}