using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcSite
{
   public class MvcApplication : System.Web.HttpApplication
   {
      protected void Application_Start ()
      {
         AreaRegistration.RegisterAllAreas();
         RegisterGlobalFilters(GlobalFilters.Filters);
         RegisterRoutes(RouteTable.Routes);
         NLogEx.Mvc.LogContext.Register();
      }
      protected void Application_End ()
      {
         NLogEx.Log.Flush();
      }
      private static void RegisterGlobalFilters (GlobalFilterCollection filters)
      {
         FilterProviders.Providers.Add(NLogEx.Mvc.Config.FilterProvider);
      }
      private static void RegisterRoutes (RouteCollection routes)
      {
         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
         // default routes
         routes.MapRoute(
             "Default", // Route name
             "{controller}/{action}/{id}", // URL with parameters
             new { controller = "Log", action = "Index", id = UrlParameter.Optional } // Parameter defaults
         );
      }
   }
}
