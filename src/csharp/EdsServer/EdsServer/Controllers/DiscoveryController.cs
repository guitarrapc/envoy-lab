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
    [Route("v2")]
    [Produces("application/json")]
    public class DiscoveryController : ControllerBase
    {
        private const string VERSION = "v1";

        private readonly EdsServiceModel _model;
        private readonly ILogger<EdsServiceController> _logger;

        public DiscoveryController(EdsServiceModel model, ILogger<EdsServiceController> logger)
        {
            _model = model;
            _logger = logger;
        }

        /// <summary>
        /// get_service_v2
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("[controller]:endpoints")]
        [HttpPost]
        public ActionResult<V2DiscoveryResponse> Get(Req request)
        {
            var resourceNames = request.ResourceNames;
            if (resourceNames == null || !resourceNames.Any())
            {
                return new BadRequestObjectResult("Service Name not provided");
            }

            var id = request?.Node?.Id;
            var cluster = request?.Node?.Cluster;

            // Even if multiple resource found, first item will return and exit.
            foreach (var r in resourceNames)
            {
                var service = _model.Get(r);
                if (service == null)
                {
                    return new V2DiscoveryResponse
                    {
                        VersionInfo = VERSION,
                        Resources = new[] {
                            new V2DiscoveryResource
                            {
                                ClusterName = r,
                            },
                        },
                    };
                }
                else
                {
                    var endpoints = service.Hosts.Select(x => new V2DiscoveryEndpointEndpoint
                    {
                        Endpoint = new V2DiscoveryEndpointAddress
                        {
                            Address = new V2DiscoveryAddress
                            {
                                SocketAddress = new V2DiscoverySocketAddress
                                {
                                    Address = x.IPAddress,
                                    PortValue = x.Port,
                                }
                            },
                        },
                    })
                    .ToArray();
                    return new V2DiscoveryResponse
                    {
                        VersionInfo = VERSION,
                        Resources = new[] {
                            new V2DiscoveryResource
                            {
                                ClusterName = r,
                                Endpoint = new [] {
                                    new Dictionary<string, V2DiscoveryEndpointEndpoint[]>
                                    {
                                        { "lb_endpoints", endpoints },
                                    }
                                },
                            }
                        },
                    };
                }
            }

            return new NotFoundObjectResult($"Service doesn't exist.");
        }
    }
}
