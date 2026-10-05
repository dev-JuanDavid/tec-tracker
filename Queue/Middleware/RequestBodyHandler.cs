

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
namespace Queue.Middleware {
    /// <summary>
    ///  RequestBodyHandler para poder leer el cuerpo de la solicitud antes de que llegue al controlador o middleware filter.
    ///  El Delegado debe registrarse en el archivo Global.asax.cs.
    /// </summary>
    public class RequestBodyHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestBody = await request.Content.ReadAsStringAsync();

            request.Properties.Add("RequestBody", requestBody);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
