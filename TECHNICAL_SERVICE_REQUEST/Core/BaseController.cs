using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TECHNICAL_SERVICE_REQUEST.Models;

namespace TECHNICAL_SERVICE_REQUEST.Core
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _db = new ApplicationDbContext();

        protected AppUserSession GetAppUserSession()
        {
            var provider = new UserSessionProvider(_db);
            var loadedSession = provider.GetCurrentUserSession(User.Identity.Name);
            if (loadedSession != null)
            {
                return loadedSession;
            }

            var currentUser = _db.AppUsers.FirstOrDefault(r => r.Email == User.Identity.Name);
            if (currentUser == null)
            {
                return null;
            }

            return new AppUserSession
            (
                id: currentUser.Id,
                firstName: currentUser.FirstName,
                lastName: currentUser.LastName,
                middleName: currentUser.MiddleName,
                extensionName: currentUser.ExtensionName,
                email: currentUser.Email,
                contactNumber: currentUser.ContactNumber,
                office: currentUser.Office,
                position: currentUser.Position,
                roleId: currentUser.RoleId
            );
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                viewName = ControllerContext.RouteData.GetRequiredString("action");
            }

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                if (viewResult.View == null)
                {
                    throw new InvalidOperationException("Partial view '" + viewName + "' was not found.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer
                );

                viewResult.View.Render(viewContext, writer);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return writer.GetStringBuilder().ToString();
            }
        }

        protected Dictionary<string, string> GetModelStateErrors()
        {
            return ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(
                    k => k.Key,
                    v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage))
                );
        }

    }
}