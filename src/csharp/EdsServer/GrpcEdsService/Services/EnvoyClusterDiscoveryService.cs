using Envoy.Api.V2;
using Envoy.Api.V2.Core;
using Envoy.Api.V2.Endpoint;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
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
    public class EnvoyClusterDiscoveryService : ClusterDiscoveryService.ClusterDiscoveryServiceBase
    {
        private readonly CdsServiceModel _model;
        private readonly ILogger<EnvoyClusterDiscoveryService> _logger;
        private readonly ServiceVersionContext _versionContext;

        public EnvoyClusterDiscoveryService(ServiceVersionContext versionContext, CdsServiceModel model, ILogger<EnvoyClusterDiscoveryService> logger)
        {
            _model = model;
            _logger = logger;
            _versionContext = versionContext;
        }

        public override async Task StreamClusters(IAsyncStreamReader<DiscoveryRequest> requestStream, IServerStreamWriter<DiscoveryResponse> responseStream, ServerCallContext context)
        {
            await requestStream.ForEachAsync(async request =>
            {
                _logger.LogInformation($"{nameof(StreamClusters)}: request coming {string.Join(",", request.ResourceNames)}");
                // todo: send timing control
                while (true)
                {
                    var response = FetchClusters(request);
                    await responseStream.WriteAsync(response);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }

        public override Task<DiscoveryResponse> FetchClusters(DiscoveryRequest request, ServerCallContext context)
        {
            var response = FetchClusters(request);
            return Task.FromResult(response);
        }


        private DiscoveryResponse FetchClusters(DiscoveryRequest request)
        {
            var response = new DiscoveryResponse
            {
                VersionInfo = _versionContext.Version + ".CDS",
                TypeUrl = "type.googleapis.com/envoy.api.v2.Cluster",
            };

            var service = _model.Gets();
            if (service.Count == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "cluster not found"));
            }
            else
            {
                foreach (var cluster in service.Values.First().Clusters)
                {
                    var resource = new Google.Protobuf.WellKnownTypes.Any
                    {
                        TypeUrl = "type.googleapis.com/envoy.api.v2.Cluster",
                        Value = cluster.ToByteString(),
                    };
                    response.Resources.Add(resource);
                }
            }

            //var clusters = CreateCluster();
            //foreach (var cluster in clusters)
            //{
            //    var resource = new Google.Protobuf.WellKnownTypes.Any
            //    {
            //        TypeUrl = "type.googleapis.com/envoy.api.v2.Cluster",
            //        Value = cluster.ToByteString(),
            //    };
            //    response.Resources.Add(resource);
            //}

            return response;
        }

        private static Cluster[] CreateCluster()
        {
            var clusterItems = new Dictionary<string, (string serviceName, string cluster)>
            {
                { "service_backend", ("myservice", "xds_cluster") },
                { "service_backend_1", ("myservice_1", "xds_cluster") },
                { "service_backend_2", ("myservice_2", "xds_cluster") },
                { "service_backend_3", ("myservice_3", "xds_cluster") },
            };

            var clusters = clusterItems.Select(x =>
            {
                var grpcService = new GrpcService
                {
                    EnvoyGrpc = new GrpcService.Types.EnvoyGrpc
                    {
                        ClusterName = x.Value.cluster,
                    }
                };
                var apiConfigSource = new ApiConfigSource
                {
                    ApiType = ApiConfigSource.Types.ApiType.Grpc,
                    TransportApiVersion = ApiVersion.V2,
                };
                apiConfigSource.GrpcServices.Add(grpcService);

                var healthCheck = new HealthCheck
                {
                    Timeout = Duration.FromTimeSpan(TimeSpan.FromSeconds(1)),
                    Interval = Duration.FromTimeSpan(TimeSpan.FromSeconds(5)),
                    UnhealthyThreshold = 1,
                    HealthyThreshold = 1,
                    HttpHealthCheck = new HealthCheck.Types.HttpHealthCheck
                    {
                        Path = "/healthz",
                    },
                };
                var cluster = new Cluster
                {
                    Name = x.Key,
                    Type = Cluster.Types.DiscoveryType.Eds,
                    ConnectTimeout = Duration.FromTimeSpan(TimeSpan.FromMilliseconds(500)),
                    DrainConnectionsOnHostRemoval = true,
                    EdsClusterConfig = new Cluster.Types.EdsClusterConfig
                    {
                        ServiceName = x.Value.serviceName,
                        EdsConfig = new ConfigSource
                        {
                            ApiConfigSource = apiConfigSource,
                        },
                    },
                };
                cluster.HealthChecks.Add(healthCheck);
                return cluster;
            })
            .ToArray();
            return clusters;
        }
    }
}
