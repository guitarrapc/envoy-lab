using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EdsServer.Models;
using EdsServer.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EdsServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class EdsServiceController : ControllerBase
    {
        private readonly EdsServiceModel _model;
        private readonly ILogger<EdsServiceController> _logger;

        public EdsServiceController(EdsServiceModel model, ILogger<EdsServiceController> logger)
        {
            _model = model;
            _logger = logger;
        }

        /// <summary>
        /// List all resources
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IDictionary<string, Service>> Get()
        {
            return new JsonResult(_model.Gets());
        }

        /// <summary>
        /// List hosts for service
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

        /// <summary>
        /// Create a given resource
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [Route("{serviceName}")]
        [HttpPost]
        public ActionResult<Service> Add(string serviceName, Service data)
        {
            if (!_model.Exists(serviceName))
            {
                _model.Add(serviceName, data);
                return new JsonResult(_model.Get(serviceName));
            }
            return new ConflictObjectResult($"Service {serviceName} already exists.");
        }

        /// <summary>
        /// Update a given resource
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [Route("{serviceName}")]
        [HttpPut]
        public ActionResult<Service> Update(string serviceName, Service data)
        {
            if (_model.Exists(serviceName))
            {
                _model.Update(serviceName, data);
                return new JsonResult(_model.Get(serviceName));
            }
            return new NotFoundObjectResult($"Service {serviceName} doesn't exist.");
        }

        /// <summary>
        /// Delete a given resource
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        [Route("{serviceName}")]
        [HttpDelete]
        public ActionResult Delete(string serviceName)
        {
            if (_model.Exists(serviceName))
            {
                _model.Delete(serviceName);
                return new NoContentResult();
            }
            return new NotFoundObjectResult($"Service {serviceName} doesn't exist.");
        }
    }
}
