using Envoy.Api.V2.Route;
using GrpcEdsService;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GrpcEdsService.Models
{
    public class RdsServiceModel
    {
        private readonly ConcurrentDictionary<string, RouteRegisterRouteConfig> _routes = new ConcurrentDictionary<string, RouteRegisterRouteConfig>();        
        private readonly ILogger<RdsServiceModel> _logger;

        public RdsServiceModel(ILogger<RdsServiceModel> logger)
        {
            _logger = logger;
        }

        public IDictionary<string, RouteRegisterRouteConfig> Gets()
        {
            return _routes.ToImmutableDictionary();
        }
        
        public RouteRegisterRouteConfig? Get(string routeName)
        {
            return _routes.TryGetValue(routeName, out var service)
                ? service
                : null;
        }

        public bool Exists(string routeName)
        {
            return _routes.TryGetValue(routeName, out var _);
        }

        public void Add(string routeName, RouteRegisterRouteConfig data)
        {
            if (!_routes.TryAdd(routeName, data))
            {
                _logger.LogInformation($"Could not add {routeName}");
            }
        }

        public void Update(string routeName, RouteRegisterRouteConfig data)
        {
            if (_routes.TryGetValue(routeName, out var currentValue))
            {
                if (!_routes.TryUpdate(routeName, data, currentValue))
                {
                    _logger.LogInformation($"Could not update {routeName}");
                }
            }
        }

        public void Delete(string serviceName)
        {
            if (!_routes.TryRemove(serviceName, out var _))
            {
                _logger.LogInformation($"Could not delete {serviceName}");
            }
        }
    }
}
