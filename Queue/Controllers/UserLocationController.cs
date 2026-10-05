using Queue.Action;
using Queue.Middleware;
using Queue.Models;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Queue.Controllers
{
    [RoutePrefix("api/UserLocation")]
    public class UserLocationController : ApiController
    {
        aUtilities result = new aUtilities();

        [HttpPost]
        [Route("Save")]
        [AgentMiddlewareFilter]
        public HttpResponseMessage UserLocation(LocationModel model)
        {
            aUserLocation service = new aUserLocation();
            return result.ReturnResponse(service.UserLocation(model));
          //  return new HttpResponseMessage();
        }
    }
}