using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes
{
    public class Authorize2 : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // If they are authorized, handle accordingly
            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                filterContext.HttpContext.Session.Abandon();
                // Otherwise redirect to your specific authorized area
                filterContext.Result = new RedirectToRouteResult(
                   new RouteValueDictionary(
                       new
                       {
                           controller = "Error",
                           action = "Unauthorized"
                       })
                   );
            }
        }
    }

    public class AuthenticateUserPrivilege : AuthorizeAttribute
    {
        private int[] AllowedPrivileges { get; set; }

        public AuthenticateUserPrivilege(int[] allowedPrivileges)
        {
            this.AllowedPrivileges = allowedPrivileges;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            using (var db = new ApplicationDbContext())
            {
                var currentUser = new UserSessionProvider(db).GetCurrentUserSession(httpContext.User.Identity.Name);
                if (currentUser == null || !AllowedPrivileges.Any(p => currentUser.PrivilegeIds.Contains(p)))
                {
                    return false;
                }
            }

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Error",
                        action = "Unauthorized"
                    })
                );
        }

    }
}