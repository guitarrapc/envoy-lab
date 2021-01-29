using Envoy.Api.V2;
using Envoy.Api.V2.Core;
using Envoy.Api.V2.Endpoint;
using Envoy.Api.V2.Route;
using Envoy.Service.Discovery.V2;
using Google.Appengine.V1;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Core.Utils;
using GrpcEdsService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrpcEdsService.Services
{
    public class EnvoyRouteDiscoveryService : RouteDiscoveryService.RouteDiscoveryServiceBase
    {
        private readonly ServiceVersionContext _versionContext;
        private readonly RdsServiceModel _model;
        private readonly ILogger<EnvoyRouteDiscoveryService> _logger;

        public EnvoyRouteDiscoveryService(ServiceVersionContext versionContext, RdsServiceModel model, ILogger<EnvoyRouteDiscoveryService> logger)
        {
            _versionContext = versionContext;
            _model = model;
            _logger = logger;
        }

        public override async Task StreamRoutes(IAsyncStreamReader<DiscoveryRequest> requestStream, IServerStreamWriter<DiscoveryResponse> responseStream, ServerCallContext context)
        {
            await requestStream.ForEachAsync(async request =>
            {
                _logger.LogInformation($"{nameof(StreamRoutes)}: request coming {string.Join(",", request.ResourceNames)}");
                while (true)
                {
                    var response = FetchRoutes(request);
                    await responseStream.WriteAsync(response);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }

        public override async Task<DiscoveryResponse> FetchRoutes(DiscoveryRequest request, ServerCallContext context)
        {
            var resourceNames = request.ResourceNames;
            if (resourceNames == null || !resourceNames.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Service Name not provided"));
            }

            var response = new DiscoveryResponse
            {
                VersionInfo = _versionContext.Version + ".RDS",
                TypeUrl = "type.googleapis.com/envoy.api.v2.RouteConfiguration",
            };

            foreach (var r in resourceNames)
            {
                var routeConfiguration = _model.Get(r);
                if (routeConfiguration == null)
                    throw new ArgumentOutOfRangeException($"route config name {r} not found.");

                var routeAny = new Google.Protobuf.WellKnownTypes.Any
                {
                    TypeUrl = "type.googleapis.com/envoy.api.v2.RouteConfiguration",
                    Value = routeConfiguration.ToByteString(),
                };
                response.Resources.Add(routeAny);
            }

            return response;
        }

        private DiscoveryResponse FetchRoutes(DiscoveryRequest request)
        {
            var resourceNames = request.ResourceNames;
            if (resourceNames == null || !resourceNames.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Service Name not provided"));
            }

            var response = new DiscoveryResponse
            {
                VersionInfo = _versionContext.Version + ".RDS",
                TypeUrl = "type.googleapis.com/envoy.api.v2.RouteConfiguration",
            };

            foreach (var r in resourceNames)
            {
                var routeConfiguration = _model.Get(r);
                if (routeConfiguration == null)
                    throw new ArgumentOutOfRangeException($"route config name {r} not found.");

                var resource = new Google.Protobuf.WellKnownTypes.Any
                {
                    TypeUrl = "type.googleapis.com/envoy.api.v2.RouteConfiguration",
                    Value = routeConfiguration.ToByteString(),
                };
                response.Resources.Add(resource);
            }

            return response;
        }

        private RouteConfiguration CreateRouteConfiguration(string r)
        {
            var headers = new Dictionary<string, (string key, string value)>
            {
                { "x-selector-1", ("x-selector", "1") },
                { "x-selector-2", ("x-selector", "2") },
                { "x-selector-3", ("x-selector", "3") },
                { "x-selector-all", ("x-selector", "all") },
                { "", ("", "") },
            };
            var virtualHost = new VirtualHost
            {
                Name = r,
            };
            virtualHost.Domains.Add("*");
            foreach (var header in headers)
            {
                var clusterName = "service_backend";
                var routeMatch = new RouteMatch
                {
                    Prefix = "/",
                };
                if (!string.IsNullOrEmpty(header.Key))
                {
                    var headerMatcher = new HeaderMatcher
                    {
                        Name = header.Value.key,
                        ExactMatch = header.Value.value,
                    };
                    routeMatch.Headers.Add(headerMatcher);
                    clusterName = clusterName + "_" + header.Value.value;
                }
                var route = new Route
                {
                    Match = routeMatch,
                    Route_ = new RouteAction
                    {
                        Cluster = clusterName,
                    },
                };
                virtualHost.Routes.Add(route);
            }
            var routeConfiguration = new RouteConfiguration
            {
                Name = r,
            };
            routeConfiguration.VirtualHosts.Add(virtualHost);
            return routeConfiguration;
        }
    }
}
