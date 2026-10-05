using Queue.Action;
using Queue.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Queue.Middleware;


namespace Queue.Controllers
{
    [RoutePrefix("api/Capture")]
    public class CaptureController : ApiController
    {
        aUtilities ut = new aUtilities();

        //[HttpPost]
        //[Route("WindowsCapture")]
        //[AgentMiddlewareFilter]
        //public HttpResponseMessage WindowsCapture(CaptureModel t)
        //{
        //    aAutomaticTakeTime s = new aAutomaticTakeTime();
        //    return ut.ReturnResponse(s.CreateCapture(t));
        //}

        [HttpPost]
        [Route("WindowsCaptureSync")]
        [AgentMiddlewareFilter]
        public HttpResponseMessage WindowsCaptureSync(List<CaptureModel> captureModel)
        {
            aAutomaticTakeTime s = new aAutomaticTakeTime();
            return ut.ReturnResponse(s.CreateCaptures(captureModel));
        }
    }
}