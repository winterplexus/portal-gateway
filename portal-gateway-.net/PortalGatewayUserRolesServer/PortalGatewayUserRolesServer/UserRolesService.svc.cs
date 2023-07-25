//
//  UserRolesService.svc.cs
//
//  Wiregrass Code Technology 2020-2023
//
using System;

[assembly: CLSCompliant(true)]
namespace PortalGatewayUserRolesServer
{
    public class UserRolesService : IUserRolesService
    {
        public string UserRoles(string userId)
        {
            var userRolesServer = new UserRolesServer();
            return userRolesServer.Search(userId);
        }            

        public int Count()
        {
            var userRolesServer = new UserRolesServer();
            return userRolesServer.Count();
        }
    }
}