using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TECHNICAL_SERVICE_REQUEST.Attributes;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;

namespace TECHNICAL_SERVICE_REQUEST.Controllers
{
    [Authorize2]
    [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
    public class LayoutController : BaseController
    {
        [ChildActionOnly]
        public ActionResult Header()
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new Exception("User not found.");
            }

            ViewBag.CurrentUser = currentUser;
            return PartialView("~/Views/Shared/_Header.cshtml");
        }
    }
}