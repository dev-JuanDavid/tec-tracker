using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;

namespace Queue
{
    public static class AccessPolicy
    {
        public const string Administrators = "SAdmin,SuperAdmin,Admin";
        public const string SuperAdministrators = "SAdmin,SuperAdmin";
        public static bool IsSuper(IPrincipal user) { return user.IsInRole("SAdmin") || user.IsInRole("SuperAdmin"); }
        public static bool CanAdminister(IPrincipal user) { return IsSuper(user) || user.IsInRole("Admin"); }
        public static bool CanReport(IPrincipal user) { return CanAdminister(user) || user.IsInRole("Employer"); }
        public static bool CanAssignRole(IPrincipal user, string role) {
            return IsSuper(user) || (CanAdminister(user) && new[] { "Admin", "Employer", "User" }.Contains(role));
        }
    }

    // MVC permissions shared with navigation; agent Web API authentication is independent.
    public class WorkspaceAccessFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext context)
        {
            var controller = Convert.ToString(context.RouteData.Values["controller"]);
            var action = Convert.ToString(context.RouteData.Values["action"]);
            var user = context.HttpContext.User;
            if (controller.Equals("Account", StringComparison.OrdinalIgnoreCase) && !action.Equals("Register", StringComparison.OrdinalIgnoreCase)) return;
            var personal = controller.Equals("Manage", StringComparison.OrdinalIgnoreCase) &&
                !new[] { "UserList", "EditUser" }.Contains(action, StringComparer.OrdinalIgnoreCase);
            if (personal) return;
            bool allowed;
            if (controller.Equals("Role", StringComparison.OrdinalIgnoreCase) || controller.Equals("Agent_Empresa", StringComparison.OrdinalIgnoreCase) || controller.Equals("Licenses", StringComparison.OrdinalIgnoreCase))
                allowed = AccessPolicy.IsSuper(user);
            else if (controller.Equals("Home", StringComparison.OrdinalIgnoreCase) || controller.Equals("ReportGantt", StringComparison.OrdinalIgnoreCase))
                allowed = AccessPolicy.CanReport(user);
            else if (controller.Equals("Operation", StringComparison.OrdinalIgnoreCase))
                allowed = AccessPolicy.CanAdminister(user) || (AccessPolicy.CanReport(user) && new[] { "TimePerActivity", "SoftwareReport", "SoftwareReportDetails", "HardwareReport", "HardwareReportDetails", "GetUserByArea" }.Contains(action, StringComparer.OrdinalIgnoreCase));
            else allowed = AccessPolicy.CanAdminister(user);
            if (allowed) return;
            if (!user.Identity.IsAuthenticated) { context.Result = new HttpUnauthorizedResult(); return; }
            if (controller.Equals("Home", StringComparison.OrdinalIgnoreCase) && action.Equals("Index", StringComparison.OrdinalIgnoreCase)) {
                context.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Manage", action = "Index" }));
                return;
            }
            context.HttpContext.Response.StatusCode = 403;
            context.HttpContext.Response.TrySkipIisCustomErrors = true;
            context.Result = new ContentResult { Content = "No tienes permisos para acceder a esta opción.", ContentType = "text/plain; charset=utf-8" };
        }
    }
}
