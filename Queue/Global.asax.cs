using log4net.Config;
using Queue.DAL;
using log4net;
using System.Diagnostics;
using System.IO;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Server.MapPath("~/log4net.config")));
            Log.Info("Inicio de la aplicación web.");
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Forzar compilación temprana de vistas (reduce "first load")
            System.Web.Compilation.BuildManager.GetReferencedAssemblies();
            Log.Info("BootstrapAdmin: Application_Start invoca EnsureCreated.");
            DefaultAdministrator.EnsureCreated();
            Log.Info("BootstrapAdmin: EnsureCreated terminó; revisar su resultado en los registros anteriores.");
            Log.Info("Aplicación lista: rutas MVC y Web API registradas.");
        }

        protected void Application_BeginRequest()
        {
            Context.Items["RequestLogId"] = Guid.NewGuid().ToString("N");
            Context.Items["RequestLogTimer"] = Stopwatch.StartNew();
            Log.InfoFormat("HTTP inicio {0} {1} requestId={2}",
                Request.HttpMethod, Request.Path, Context.Items["RequestLogId"]);
        }

        protected void Application_PostAuthenticateRequest()
        {
            Log.InfoFormat("HTTP autenticación authenticated={0} requestId={1}",
                User != null && User.Identity != null && User.Identity.IsAuthenticated,
                Context.Items["RequestLogId"]);
        }

        protected void Application_AcquireRequestState()
        {
            Log.InfoFormat("HTTP sesión available={0} newSession={1} hasUser={2} hasCompany={3} requestId={4}",
                Context.Session != null, Context.Session != null && Context.Session.IsNewSession,
                Context.Session != null && Context.Session["UserId"] != null,
                Context.Session != null && Context.Session["Company"] != null,
                Context.Items["RequestLogId"]);
        }

        protected void Application_EndRequest()
        {
            var timer = Context.Items["RequestLogTimer"] as Stopwatch;
            Log.InfoFormat("HTTP {0} {1} status={2} durationMs={3} requestId={4}",
                Request.HttpMethod, Request.Path, Response.StatusCode,
                timer == null ? 0 : timer.ElapsedMilliseconds, Context.Items["RequestLogId"]);
        }

        protected void Application_Error()
        {
            Log.Error(string.Format("Error HTTP {0} {1} requestId={2}",
                Request.HttpMethod, Request.Path, Context.Items["RequestLogId"]), Server.GetLastError());
        }


    }
}
