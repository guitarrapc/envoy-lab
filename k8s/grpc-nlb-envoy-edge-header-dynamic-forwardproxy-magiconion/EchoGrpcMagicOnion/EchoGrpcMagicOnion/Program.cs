using EchoGrpcMagicOnion.Shared;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Hosting;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EchoGrpcMagicOnion
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            await MagicOnionHost.CreateDefaultBuilder()
                .UseMagicOnion()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<MagicOnionHostingOptions>(options =>
                    {
                        options.Service.GlobalStreamingHubFilters.Add<StreamingHeaderFilterAttribute>();
                    });
                })
                .RunConsoleAsync();
        }
    }

    public class HealthzService : ServiceBase<IHealthzService>, IHealthzService
    {
        public UnaryResult<int> Readiness()
        {
            return UnaryResult<int>(0);
        }
    }

    [FromTypeFilter(typeof(UnaryHeaderFilterAttribute))]
    public class EchoService : ServiceBase<IEchoService>, IEchoService
    {
        public async UnaryResult<string> EchoAsync(string request)
        {
            Logger.Debug($"Handling Echo request '{request}' with context {Context}");
            var hostName = Environment.MachineName;
            var metadata = new Metadata();
            metadata.Add("hostname", hostName);
            await Context.CallContext.WriteResponseHeadersAsync(metadata);

            return hostName;
        }
    }

    public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>, IMyHub
    {
        private Player player;
        private IGroup room;
        private IInMemoryStorage<Player> storage;

        public async Task<Player[]> JoinAsync(string roomName, string username)
        {
            Logger.Debug($"Handling Stream Join request '{roomName}/{username}' with context {Context}");

            player = new Player
            {
                Name = username
            };

            (room, storage) = await Group.AddAsync(roomName, player);
            Broadcast(room).OnJoin(player);

            return storage.AllValues.ToArray();
        }

        public async Task LeaveAsync()
        {
            Logger.Debug($"Handling Stream Leave request '{room.GroupName}/{player.Name}' with context {Context}");
            await room.RemoveAsync(this.Context);
            Broadcast(room).OnLeave(player);
        }

        public async Task<string> EchoAsync(string message)
        {
            Logger.Debug($"Handling Stream Echo request '{room.GroupName}/{player.Name}' with context {Context}");
            var hostName = Environment.MachineName;
            Broadcast(room).OnEcho(hostName);
            return hostName;
        }

        protected override async ValueTask OnConnecting()
        {
            Logger.Debug($"OnConnecting {Context.ContextId}");
            var metadata = new Metadata();
            var headers = Context.CallContext.RequestHeaders;
            if (!headers.Any(x => x.Key == "hostname"))
            {
                var hostName = Environment.MachineName;
                headers.Add("hostname", hostName);
                await Context.CallContext.WriteResponseHeadersAsync(metadata);
            }
        }
        protected override ValueTask OnDisconnected()
        {
            Logger.Debug($"OnDisconnected {Context.ContextId}");
            return CompletedTask;
        }
    }

    public class StreamingHeaderFilterAttribute : StreamingHubFilterAttribute
    {
        private readonly ILogger<StreamingHeaderFilterAttribute> _logger;

        public StreamingHeaderFilterAttribute(ILogger<StreamingHeaderFilterAttribute> logger)
        {
            _logger = logger;
        }
        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            _logger.LogInformation($"Request header contents of {context.Path}.");
            _logger.LogInformation("{");
            foreach (var header in context.ServiceContext.CallContext.RequestHeaders)
            {
                _logger.LogInformation($"  {header.Key}: {header.Value}");
            }
            _logger.LogInformation("}");
            await next(context);
        }
    }

    public class UnaryHeaderFilterAttribute : MagicOnionFilterAttribute
    {
        private readonly ILogger<UnaryHeaderFilterAttribute> _logger;

        public UnaryHeaderFilterAttribute(ILogger<UnaryHeaderFilterAttribute> logger)
        {
            _logger = logger;
        }
        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            _logger.LogInformation($"Request header contents of {context.CallContext.Method}.");
            _logger.LogInformation("{");
            foreach (var header in context.CallContext.RequestHeaders)
            {
                _logger.LogInformation($"  {header.Key}: {header.Value}");
            }
            _logger.LogInformation("}");
            await next(context);
        }
    }
}
