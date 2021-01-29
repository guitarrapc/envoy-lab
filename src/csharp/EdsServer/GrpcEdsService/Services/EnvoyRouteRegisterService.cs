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
    public class EnvoyRouteRegisterService : RouteRegisterService.RouteRegisterServiceBase
    {
        private readonly RdsServiceModel _model;
        private readonly ILogger<EnvoyRouteRegisterService> _logger;

        public EnvoyRouteRegisterService(RdsServiceModel model, ILogger<EnvoyRouteRegisterService> logger)
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
        public override Task<RouteRegisterListResponse> List(Empty request, ServerCallContext context)
        {
            var route = new Google.Protobuf.Collections.MapField<string, RouteRegisterRouteConfig>
            {
               _model.Gets(),
            };
            var response = new RouteRegisterListResponse();
            response.Routes.Add(route);
            return Task.FromResult(response);
        }

        /// <summary>
        /// List configuration for routes
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RouteRegisterRouteConfig> Get(RouteRegisterGetRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.RouteName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.RouteName)} property missing on request body."));
            var route = _model.Get(request.RouteName);
            if (route != null)
            {
                return Task.FromResult(route);
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Route {request.RouteName} doesn't exist."));
        }

        /// <summary>
        /// Create a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RouteRegisterRouteConfig> Add(RouteRegisterAddRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.RouteName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.RouteName)} property missing on request body."));
            if (!_model.Exists(request.RouteName))
            {
                _model.Add(request.RouteName, request.Route);
                return Task.FromResult(_model.Get(request.RouteName)!);
            }
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Route {request.RouteName} already exists."));
        }

        /// <summary>
        /// Update a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RouteRegisterRouteConfig> Update(RouteRegisterUpdateRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.RouteName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.RouteName)} property missing on request body."));
            
            if (_model.Exists(request.RouteName))
            {
                _model.Update(request.RouteName, request.Route);
                return Task.FromResult(_model.Get(request.RouteName)!);
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Route {request.RouteName} doesn't exist."));
        }

        /// <summary>
        /// Delete a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<Empty> Delete(RouteRegisterDeleteRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.RouteName))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.RouteName)} property missing on request body."));
            if (_model.Exists(request.RouteName))
            {
                _model.Delete(request.RouteName);
                return Task.FromResult(new Empty());
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Route {request.RouteName} doesn't exist."));
        }
    }
}
