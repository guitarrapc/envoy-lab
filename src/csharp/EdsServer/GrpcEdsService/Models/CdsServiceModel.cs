using GrpcEdsService;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GrpcEdsService.Models
{
    public class CdsServiceModel
    {
        private readonly ConcurrentDictionary<string, ClusterRegisterServiceItem> _services = new ConcurrentDictionary<string, ClusterRegisterServiceItem>();
        private readonly ILogger<CdsServiceModel> _logger;

        public CdsServiceModel(ILogger<CdsServiceModel> logger)
        {
            _logger = logger;
        }

        public IDictionary<string, ClusterRegisterServiceItem> Gets()
        {
            return _services.ToImmutableDictionary();
        }
        
        public ClusterRegisterServiceItem? Get(string serviceName)
        {
            return _services.TryGetValue(serviceName, out var service)
                ? service
                : null;
        }

        public bool Exists(string serviceName)
        {
            return _services.TryGetValue(serviceName, out var _);
        }

        public void Add(string serviceName, ClusterRegisterServiceItem data)
        {
            if (!_services.TryAdd(serviceName, data))
            {
                _logger.LogInformation($"Could not add {serviceName}");
            }
        }

        public void Update(string serviceName, ClusterRegisterServiceItem data)
        {
            if (_services.TryGetValue(serviceName, out var currentValue))
            {
                if (!_services.TryUpdate(serviceName, data, currentValue))
                {
                    _logger.LogInformation($"Could not update {serviceName}");
                }
            }
        }

        public void Delete(string serviceName)
        {
            if (!_services.TryRemove(serviceName, out var _))
            {
                _logger.LogInformation($"Could not delete {serviceName}");
            }
        }
    }
}
