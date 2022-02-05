//
//  AccountController.cs
//
//  Wiregrass Code Technology 2020-2022 
//
using System;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PortalGateway.Log;
using PortalGateway.Security;
using PortalGateway.Utility;

namespace PortalGateway.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnLocation)
        {
            if (!ModelState.IsValid || model == null)
            {
                return View(model);
            }
            if ((string.IsNullOrEmpty(model.UserId) && string.IsNullOrEmpty(model.Password)))
            {
                return View(model);
            }

            if (!SecurityManager.Authenticated(model.UserId, model.Password))
            {
                UsersLog.WriteActivity(string.Format(CultureInfo.InvariantCulture, "invalid user ID or password for user ID: {0}", model.UserId.ToUpperInvariant()));

                ModelState.AddModelError("ErrorMessage", "User name or password is incorrect.");

                return View(model);
            }

            UsersLog.WriteActivity(string.Format(CultureInfo.InvariantCulture, "user ID {0} has been authenticated", model.UserId.ToUpperInvariant()));

            var userRoles = SecurityManager.UserRoles(model.UserId);

            if (string.IsNullOrEmpty(userRoles))
            {
                UsersLog.WriteActivity(string.Format(CultureInfo.InvariantCulture, "user ID {0} was not found in user roles file (system error)", model.UserId.ToUpperInvariant()));

                ModelState.AddModelError("ErrorMessage", "System error.");

                return View(model);
            }
            if (!string.IsNullOrEmpty(userRoles))
            {
                if (userRoles == "system error" || userRoles == "user ID is missing or empty")
                {
                    ModelState.AddModelError("ErrorMessage", "Internal application error.");

                    return View(model);
                }
                if (userRoles == "user ID has no user roles")
                {
                    UsersLog.WriteActivity(string.Format(CultureInfo.InvariantCulture, "user ID {0} was not found in user roles file", model.UserId.ToUpperInvariant()));

                    ModelState.AddModelError("ErrorMessage", "You are not authorized to use this application.");

                    return View(model);
                }
            }

            UsersLog.WriteActivity(string.Format(CultureInfo.InvariantCulture, "user ID {0} has been authorized with the following roles: {1} ", model.UserId.ToUpperInvariant(), userRoles));

            SetFormsAuthentication(model.UserId, userRoles);
            if (Url.IsLocalUrl(returnLocation) && IsValidReturnLocation(returnLocation))
            {
                return Redirect(returnLocation);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        private void SetFormsAuthentication(string userId, string userRoles)
        {
            var expirationTime = 15;

            var expirationTimeValue = Assistant.GetConfigurationValue("CookieExpirationTime");
            if (!string.IsNullOrEmpty(expirationTimeValue))
            {
                if (!int.TryParse(expirationTimeValue, out expirationTime))
                {
                    WindowsEventLog.WriteEntry(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "invalid expiration time configuration value: " + expirationTimeValue);
                }
            }

            var ticket = new FormsAuthenticationTicket(1, userId, DateTime.Now, DateTime.Now.AddMinutes(expirationTime), true, userRoles, FormsAuthentication.FormsCookiePath);

            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));
        }

        private static bool IsValidReturnLocation(string location)
        {
            if (location.Length < 1)
            {
                return false;
            }
            if (location.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                if (!location.StartsWith("//", StringComparison.OrdinalIgnoreCase) && !location.StartsWith("/\\", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}