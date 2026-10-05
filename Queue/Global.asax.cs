using log4net.Config;
using Queue.DAL;
using Queue.Middleware;
using Queue.Migrations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Queue
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Forzar compilación temprana de vistas (reduce "first load")
            System.Web.Compilation.BuildManager.GetReferencedAssemblies();
        }


    }
}
