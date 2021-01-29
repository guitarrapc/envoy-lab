using Google.Appengine.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcEdsService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcEdsService.Services
{
    public class EnvoyClusterRegisterService : ClusterRegisterService.ClusterRegisterServiceBase
    {
        private readonly CdsServiceModel _model;
        private readonly ILogger<EnvoyClusterRegisterService> _logger;

        public EnvoyClusterRegisterService(CdsServiceModel model, ILogger<EnvoyClusterRegisterService> logger)
        {
            _model = model;
            _logger = logger;
        }

        /// <summary>
        /// List all resources
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ClusterRegisterListResponse> List(Empty request, ServerCallContext context)
        {
            var service = new Google.Protobuf.Collections.MapField<string, ClusterRegisterServiceItem>
            {
               _model.Gets(),
            };
            var response = new ClusterRegisterListResponse();
            response.Services.Add(service);
            return Task.FromResult(response);
        }

        /// <summary>
        /// List clusters for service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ClusterRegisterServiceItem> Get(ClusterRegisterGetRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.ServiceName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.ServiceName)} property missing on request body."));
            var service = _model.Get(request.ServiceName);
            if (service != null)
            {
                return Task.FromResult(service);
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Service {request.ServiceName} doesn't exist."));
        }

        /// <summary>
        /// Create a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ClusterRegisterServiceItem> Add(ClusterRegisterAddRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.ServiceName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.ServiceName)} property missing on request body."));
            if (!_model.Exists(request.ServiceName))
            {
                _model.Add(request.ServiceName, request.Service);
                return Task.FromResult(_model.Get(request.ServiceName)!);
            }
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Service {request.ServiceName} already exists."));
        }

        /// <summary>
        /// Update a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ClusterRegisterServiceItem> Update(ClusterRegisterUpdateRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.ServiceName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.ServiceName)} property missing on request body."));
            if (_model.Exists(request.ServiceName))
            {
                _model.Update(request.ServiceName, request.Service);
                return Task.FromResult(_model.Get(request.ServiceName)!);
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Service {request.ServiceName} doesn't exist."));
        }

        /// <summary>
        /// Delete a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<Empty> Delete(ClusterRegisterDeleteRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.ServiceName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.ServiceName)} property missing on request body."));
            if (_model.Exists(request.ServiceName))
            {
                _model.Delete(request.ServiceName);
                return Task.FromResult(new Empty());
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Service {request.ServiceName} doesn't exist."));
        }
    }
}
