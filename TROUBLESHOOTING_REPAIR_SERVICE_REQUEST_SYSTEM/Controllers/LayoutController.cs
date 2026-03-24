using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    [Authorize2]
    [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
    public class LayoutController : BaseController
    {
        [ChildActionOnly]
        public ActionResult Header()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new Exception("User not found.");
            }

            ViewBag.CurrentUser = currentUser;
            return PartialView("~/Views/Shared/_Header.cshtml");
        }
    }
}