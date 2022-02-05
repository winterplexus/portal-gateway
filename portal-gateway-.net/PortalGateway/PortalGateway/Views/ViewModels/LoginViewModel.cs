//
//  LoginViewModel.cs
//
//  Wiregrass Code Technology 2020-2022 
//
using System.ComponentModel.DataAnnotations;

namespace PortalGateway
{
    public class LoginViewModel
    {
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}