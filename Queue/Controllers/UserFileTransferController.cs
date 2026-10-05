using Queue.Action;
using Queue.Middleware;
using Queue.Models;
using System.Net.Http;
using System.Web.Http;

namespace Queue.Controllers
{
    [RoutePrefix("api/UserFileTransfer")]
    public class UserFileTransferController : ApiController
    {
        aUtilities result = new aUtilities();

        [HttpPost]
        [Route("Save")]
        [AgentMiddlewareFilter]
        public HttpResponseMessage SaveFileTranfer(FileTransferModel model)
        {
            aFileTransfer servicio = new aFileTransfer();
            return result.ReturnResponse(servicio.SaveFileTransfer(model));
        }
    }
}