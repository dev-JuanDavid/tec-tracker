using Queue.Utils;
using System.Web.Mvc;

namespace Queue.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {

            public void Warning(string messagecode, string aditional)
            {
                if (!string.IsNullOrEmpty(aditional))
                    TempData[Alerts.WARNING] = messagecode + " Detalle: " + aditional;
                else
                    TempData[Alerts.WARNING] = messagecode;
            }

            public void Success(string messagecode)
            {
                TempData[Alerts.SUCCESS] = messagecode;
            }

            public void Information(string messagecode)
            {
                TempData[Alerts.INFORMATION] = messagecode;
            }

            public void Error(string messagecode, string aditional)
            {
                if (!string.IsNullOrEmpty(aditional))
                    TempData[Alerts.ERROR] = messagecode + " Detalle: " + aditional;
                else
                    TempData[Alerts.ERROR] = messagecode;
            }
        

    }
}
