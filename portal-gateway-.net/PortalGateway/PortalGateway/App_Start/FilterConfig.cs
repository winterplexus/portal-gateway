//
//  FilterConfig.cs
//
//  Wiregrass Code Technology 2020-2023 
//
using System.Web.Mvc;

namespace PortalGateway
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}