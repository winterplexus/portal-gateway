//
//  IUserRolesService.cs
//
//  Wiregrass Code Technology 2020-2023
//
using System.ServiceModel;
using System.ServiceModel.Web;

namespace PortalGatewayUserRolesServer
{
    [ServiceContract]
    public interface IUserRolesService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "?UserId={userId}")]
        string UserRoles(string userId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/count")]
        int Count();
    }
}