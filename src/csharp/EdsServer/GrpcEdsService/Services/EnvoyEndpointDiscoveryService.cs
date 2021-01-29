using Envoy.Api.V2;
using Envoy.Api.V2.Core;
using Envoy.Api.V2.Endpoint;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Core.Logging;
using Grpc.Core.Utils;
using GrpcEdsService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcEdsService.Services
{
    public class EnvoyEndpointDiscoveryService : EndpointDiscoveryService.EndpointDiscoveryServiceBase
    {
        private readonly EdsServiceModel _model;
        private readonly ILogger<EnvoyEndpointDiscoveryService> _logger;
        private readonly ServiceVersionContext _versionContext;

        public EnvoyEndpointDiscoveryService(ServiceVersionContext versionContext, EdsServiceModel model, ILogger<EnvoyEndpointDiscoveryService> logger)
        {
            _model = model;
            _logger = logger;
            _versionContext = versionContext;
        }

        public override async Task StreamEndpoints(IAsyncStreamReader<DiscoveryRequest> requestStream, IServerStreamWriter<DiscoveryResponse> responseStream, ServerCallContext context)
        {
            await requestStream.ForEachAsync(async request =>
            {
                _logger.LogInformation($"{nameof(StreamEndpoints)}: request coming {string.Join(",", request.ResourceNames)}");
                // todo: send timing control
                while (true)
                {
                    var response = FetchEndpoints(request);
                    await responseStream.WriteAsync(response);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }

        public override Task<DiscoveryResponse> FetchEndpoints(DiscoveryRequest request, ServerCallContext context)
        {
            var response = FetchEndpoints(request);
            return Task.FromResult(response);
        }

        private DiscoveryResponse FetchEndpoints(DiscoveryRequest request)
        {
            var resourceNames = request.ResourceNames;
            if (resourceNames == null || !resourceNames.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Service Name not provided"));
            }

            var id = request?.Node?.Id;
            var cluster = request?.Node?.Cluster;
            var response = new DiscoveryResponse
            {
                VersionInfo = _versionContext.Version + ".EDS",
                TypeUrl = "type.googleapis.com/envoy.api.v2.ClusterLoadAssignment",
            };

            foreach (var r in resourceNames)
            {
                var service = _model.Get(r);
                if (service == null)
                {
                    var cla = new ClusterLoadAssignment
                    {
                        ClusterName = r,
                    };
                    response.Resources.Add(new Google.Protobuf.WellKnownTypes.Any
                    {
                        Value = cla.ToByteString(),
                    });
                }
                else
                {
                    var cla = new ClusterLoadAssignment
                    {
                        ClusterName = r,
                    };
                    foreach (var host in service.Hosts)
                    {
                        var lep = new LocalityLbEndpoints
                        {
                            LoadBalancingWeight = host.Tags.LoadBalancingWeight,
                        };
                        lep.LbEndpoints.Add(new RepeatedField<LbEndpoint>
                        {
                            new LbEndpoint
                            {
                                Endpoint = new Endpoint
                                {
                                    Address = new Address
                                    {
                                        SocketAddress = new SocketAddress
                                        {
                                            Address = host.IpAddress,
                                            PortValue = host.Port,
                                        }
                                    },
                                },
                            },
                        });
                        cla.Endpoints.Add(lep);
                    }

                    var resource = new Google.Protobuf.WellKnownTypes.Any
                    {
                        TypeUrl = "type.googleapis.com/envoy.api.v2.ClusterLoadAssignment",
                        Value = cla.ToByteString(),
                    };
                    response.Resources.Add(resource);
                }
            }
            return response;
        }
    }
}
