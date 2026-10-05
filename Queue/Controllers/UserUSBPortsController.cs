using Queue.Action;
using Queue.Middleware;
using Queue.Models;
using System.Net.Http;
using System.Web.Http;

namespace Queue.Controllers
{
    [RoutePrefix("api/UserUSBPorts")]
    public class UserUSBPortsController : ApiController
    {
        aUtilities result = new aUtilities();

        [HttpPost]
        [Route("Inventory")]
        [AgentMiddlewareFilter]
        public HttpResponseMessage InventoryPorts(USBPortsModel model)
        {
            aPortsMachine service = new aPortsMachine();
            return result.ReturnResponse(service.InventoryPorts(model));
        }
    }
}