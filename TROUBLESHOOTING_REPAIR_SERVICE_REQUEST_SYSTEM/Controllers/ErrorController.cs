using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ActionResult Unauthorized()
        {
            Response.StatusCode = 403;
            return View();
        }

        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}