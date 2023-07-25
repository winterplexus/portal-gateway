//
//  Global.asax
//
//  Wiregrass Code Technology 2020-2023 
//
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

[assembly: CLSCompliant(true)]
namespace PortalGateway
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}