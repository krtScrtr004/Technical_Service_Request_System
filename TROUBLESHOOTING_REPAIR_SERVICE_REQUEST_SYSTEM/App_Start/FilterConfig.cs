using System.Web;
using System.Web.Mvc;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
