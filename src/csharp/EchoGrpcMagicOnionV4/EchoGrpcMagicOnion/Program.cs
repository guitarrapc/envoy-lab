using EchoGrpcMagicOnion.Shared;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using ZLogger;

namespace EchoGrpcMagicOnion
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddZLoggerConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options =>
                        {
                            // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
                            options.ConfigureEndpointDefaults(endpointOptions =>
                            {
                                endpointOptions.Protocols = HttpProtocols.Http2;
                            });
                        })
                        .UseStartup<Startup>();
                });
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(); // MagicOnion depends on ASP.NET Core gRPC service.
            services.AddGrpcReflection();

            services.AddMagicOnion(options =>
            {
                options.GlobalStreamingHubFilters.Add<StreamingHeaderFilterAttribute>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService();
                endpoints.MapGrpcService<HealtchCheckService>();
                //endpoints.MapGrpcService<AlbService>();

                if (env.IsDevelopment())
                {
                    endpoints.MapGrpcReflectionService();
                }
            });
        }
    }

    /// <summary>
    /// You don't need implement this service if status code 12 is enough.
    /// `grpcurl -v -plaintext localhost:80 AWS.ALB/healthcheck`
    /// </summary>
    public class AlbService : ALB.ALBBase
    {
        private readonly ILogger<AlbService> _logger;

        public AlbService(ILogger<AlbService> logger)
        {
            _logger = logger;
        }

        public override Task<Google.Protobuf.WellKnownTypes.Empty> healthcheck(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            _logger.LogInformation("healthy");
            return Task.FromResult(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }

    /// <summary>
    /// Implement https://github.com/grpc/grpc/tree/master/src/csharp/Grpc.HealthCheck
    /// `grpcurl -plaintext 127.0.0.1:80 grpc.health.v1.Health.Check`
    /// `grpc-health-probe -addr 127.0.0.1:80`
    /// </summary>
    public class HealtchCheckService : Grpc.Health.V1.Health.HealthBase
    {
        public override Task<Grpc.Health.V1.HealthCheckResponse> Check(Grpc.Health.V1.HealthCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Grpc.Health.V1.HealthCheckResponse
            {
                Status = Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.Serving,
            });
        }
    }

    public class HealthzService : ServiceBase<IHealthzService>, IHealthzService
    {
        private readonly ILogger<HealthzService> _logger;
        public HealthzService(ILogger<HealthzService> logger)
        {
            _logger = logger;
        }

        public UnaryResult<int> Readiness()
        {
            return UnaryResult<int>(0);
        }
    }

    [FromTypeFilter(typeof(UnaryHeaderFilterAttribute))]
    public class EchoService : ServiceBase<IEchoService>, IEchoService
    {
        private readonly ILogger<EchoService> _logger;
        public EchoService(ILogger<EchoService> logger)
        {
            _logger = logger;
        }
        public async UnaryResult<string> EchoAsync(string request)
        {
            _logger.LogDebug($"Handling Echo request '{request}' with context {Context}");
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
        private readonly ILogger<MyHub> _logger;

        public MyHub(ILogger<MyHub> logger)
        {
            _logger = logger;
        }

        public async Task<Player[]> JoinAsync(string roomName, string username)
        {
            _logger.LogDebug($"Handling Stream Join request '{roomName}/{username}' with context {Context}");

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
            _logger.LogDebug($"Handling Stream Leave request '{room.GroupName}/{player.Name}' with context {Context}");
            await room.RemoveAsync(this.Context);
            Broadcast(room).OnLeave(player);
        }

        public Task<string> EchoAsync(string message)
        {
            _logger.LogDebug($"Handling Stream Echo request '{room.GroupName}/{player.Name}' with context {Context}");
            var hostName = Environment.MachineName;
            Broadcast(room).OnEcho(hostName);
            return Task.FromResult(hostName);
        }

        protected override async ValueTask OnConnecting()
        {
            _logger.LogDebug($"OnConnecting {Context.ContextId}");
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
            _logger.LogDebug($"OnDisconnected {Context.ContextId}");
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
