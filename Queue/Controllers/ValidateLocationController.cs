using Queue.Action;
using Queue.Models;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Queue.Controllers
{
    [RoutePrefix("api/ValidateLocation")]
    public class ValidateLocationController : ApiController
    {
        aUtilities result = new aUtilities();

        [HttpPost]
        [Route("Get")]
        public HttpResponseMessage Validate(LocationModel model)
        {
            aUserLocation service = new aUserLocation();
            return result.ReturnResponse(service.Validate(model));
            
        }
    }
}