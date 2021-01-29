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
    public class EnvoyEndpointRegisterService : EndpointRegisterService.EndpointRegisterServiceBase
    {
        private readonly EdsServiceModel _model;
        private readonly ILogger<EnvoyEndpointRegisterService> _logger;

        public EnvoyEndpointRegisterService(EdsServiceModel model, ILogger<EnvoyEndpointRegisterService> logger)
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
        public override Task<EndpointRegisterListResponse> List(Empty request, ServerCallContext context)
        {
            var service = new Google.Protobuf.Collections.MapField<string, EndpointRegisterServiceItem>
            {
               _model.Gets(),
            };
            var response = new EndpointRegisterListResponse();
            response.Services.Add(service);
            return Task.FromResult(response);
        }

        /// <summary>
        /// List hosts for service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<EndpointRegisterServiceItem> Get(EndpointRegisterGetRequest request, ServerCallContext context)
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
        public override Task<EndpointRegisterServiceItem> Add(EndpointRegisterAddRequest request, ServerCallContext context)
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
        public override Task<EndpointRegisterServiceItem> Update(EndpointRegisterUpdateRequest request, ServerCallContext context)
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
        public override Task<Empty> Delete(EndpointRegisterDeleteRequest request, ServerCallContext context)
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
