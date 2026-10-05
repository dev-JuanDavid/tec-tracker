using Queue.Action;
using Queue.DataBase;
using Queue.Models.ResponseDTO;
using Queue.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;


namespace Queue.Controllers
{
    [RoutePrefix("api/UserFunctionality")]
    public class UserFunctionalityController: ApiController
    {

        aFunctionality aFunctionality = new aFunctionality();

        [HttpPost]
        public HttpResponseMessage GetAllFunctionalities(FunctionalityDTO model)
        {
            return aFunctionality.GetAllFunctionality(model);
        }
    }
}