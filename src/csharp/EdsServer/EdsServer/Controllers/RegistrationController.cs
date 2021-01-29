using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EdsServer.Models;
using EdsServer.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EdsServer.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    [Produces("application/json")]
    public class RegistrationController : ControllerBase
    {
        private readonly EdsServiceModel _model;
        private readonly ILogger<EdsServiceController> _logger;

        public RegistrationController(EdsServiceModel model, ILogger<EdsServiceController> logger)
        {
            _model = model;
            _logger = logger;
        }

        /// <summary>
        /// get_service_v1
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        [Route("{serviceName}")]
        [HttpGet]
        public ActionResult<Service> Get(string serviceName)
        {
            var service = _model.Get(serviceName);
            if (service != null)
            {
                return new JsonResult(service);
            }
            return new NotFoundObjectResult($"Service {serviceName} doesn't exist.");
        }
    }
}
