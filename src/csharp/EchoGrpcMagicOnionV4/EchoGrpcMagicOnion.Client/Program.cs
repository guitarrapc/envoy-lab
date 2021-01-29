using ConsoleAppFramework;
using EchoGrpcMagicOnion.Shared;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoGrpcMagicOnion.Client
{
    class Program : ConsoleAppBase, IMyHubReceiver
    {
        static async Task Main(string[] args)
        {
            // local

            // -- Load
            //args = new[] {
            //    "Load",
            //    "-hostPort",
            //    "https://127.0.0.1:5000",
            //    "-H",
            //    "x-host-port: 10-0-1-19",
            //    "-verbose",
            //    "true"
            //};

            // -- Echo
            //args = new[] {
            //    "Echo",
            //    "-hostPort",
            //    "https://127.0.0.1:5000",
            //    "-message",
            //    "echo",
            //    "-H",
            //    "x-host-name: echo-grpc-0",
            //    "-verbose",
            //    "true"
            //};

            // -- Stream
            //args = new[] {
            //    "Stream",
            //    "-hostPort",
            //    "http://127.0.0.1:5000",
            //    "-roomName",
            //    "A",
            //    "-userName",
            //    "hoge",
            //    "-H",
            //    "x-pod-name: echo-grpc-0"
            //};

            // --readiness
            //args = "Readiness -hostPort http://127.0.0.1:5000".Split(" ");
            //args = "Readiness -hostPort https://envoytest.eks-sandbox.dev.cysharp.io:443".Split(" ");

            await Host.CreateDefaultBuilder()
                .RunConsoleAppFrameworkAsync<Program>(args);
        }

        [Command("Load")]
        public async Task Echo(string hostPort, [Option("H")] string headers, [Option("v")] bool verbose = false)
        {
            // under .NET Core 3.x: https://docs.microsoft.com/ja-jp/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.1#call-insecure-grpc-services-with-net-core-client
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            await Task.Delay(TimeSpan.FromSeconds(3));

            var channel = GrpcChannel.ForAddress(hostPort);
            for (var i = 0; i < 10000; i++)
            {
                var client = MagicOnionClient.Create<IEchoService>(channel, new IClientFilter[]
                {
                new AppendHeaderFilter(headers),
                new ResponseHandlingFilter(verbose),
                });

                try
                {
                    var res = await client.EchoAsync(i.ToString());
                    Console.WriteLine(res);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message} {ex.StackTrace}");
                }
            }
        }

        [Command("Echo")]
        public async Task Echo(string hostPort, string message, [Option("H")]string headers, [Option("v")]bool verbose = false)
        {
            // under .NET Core 3.x: https://docs.microsoft.com/ja-jp/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.1#call-insecure-grpc-services-with-net-core-client
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            await Task.Delay(TimeSpan.FromSeconds(3));

            var channel = GrpcChannel.ForAddress(hostPort);
            var client = MagicOnionClient.Create<IEchoService>(channel, new IClientFilter[]
            {
                new AppendHeaderFilter(headers),
                new ResponseHandlingFilter(verbose),
            });

            try
            {
                var res = await client.EchoAsync(message);
                Console.WriteLine(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} {ex.StackTrace}");
            }
        }

        [Command("Stream")]
        public async Task Stream(string hostPort, string roomName, string userName, [Option("H")]string headers)
        {
            // under .NET Core 3.x: https://docs.microsoft.com/ja-jp/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.1#call-insecure-grpc-services-with-net-core-client
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            await Task.Delay(TimeSpan.FromSeconds(3));
            var channel = GrpcChannel.ForAddress(hostPort);

            var options = new CallOptions();
            if (!string.IsNullOrEmpty(headers))
            {
                var metadata = new Metadata();
                foreach (var headerItem in headers.Split(','))
                {
                    var splitHeader = headerItem.Split(':');
                    var headerKey = splitHeader[0].Trim();
                    var headerValue = splitHeader[1].Trim();
                    metadata.Add(headerKey, headerValue);
                }
                options = options.WithHeaders(metadata);
            }
            await ConnectAndLeave(channel, roomName, userName, TimeSpan.FromSeconds(2), options);
        }

        [Command("Readiness")]
        public async Task Readiness(string hostPort)
        {
            // under .NET Core 3.x: https://docs.microsoft.com/ja-jp/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.1#call-insecure-grpc-services-with-net-core-client
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var channel = GrpcChannel.ForAddress(hostPort);
            var client = MagicOnionClient.Create<IHealthzService>(channel);
            for (var i = 0; i < 1000; i++)
                await client.Readiness();
        }

        // StreamingHub
        IMyHub _hubClient;

        public async Task ConnectAndLeave(GrpcChannel channel, string roomName, string userName, TimeSpan delay, CallOptions option)
        {
            try
            {
                await ConnectAsync(channel, roomName, userName, option);
                await EchoAsync("echo1");
                await EchoAsync("echo2");
                await EchoAsync("echo3");
                await Task.Delay(delay);
                await LeaveAsync();
            }
            finally
            {
                await _hubClient.DisposeAsync();
                await _hubClient.WaitForDisconnect();
                await channel.ShutdownAsync();
            }
        }

        public async Task ConnectAsync(GrpcChannel channel, string roomName, string userName, CallOptions option)
        {
            _hubClient = StreamingHubClient.Connect<IMyHub, IMyHubReceiver>(channel, this, option: option);
            var roomPlayers = await _hubClient.JoinAsync(roomName, userName);
            foreach (var player in roomPlayers)
            {
                (this as IMyHubReceiver).OnJoin(player);
            }
        }

        public Task LeaveAsync()
        {
            return _hubClient.LeaveAsync();
        }

        public Task<string> EchoAsync(string message)
        {
            return _hubClient.EchoAsync(message);
        }

        public void OnJoin(Player player)
        {
        }

        public void OnLeave(Player player)
        {
        }

        public void OnEcho(string message)
        {
            Console.WriteLine(message);
        }

        // filters

        public class AppendHeaderFilter : IClientFilter
        {
            private readonly string _headers;
            public AppendHeaderFilter(string headers)
            {
                _headers = headers;
            }
            public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
            {
                if (!string.IsNullOrEmpty(_headers))
                {
                    var header = context.CallOptions.Headers;
                    foreach (var headerItem in _headers.Split(','))
                    {
                        var splitHeader = headerItem.Split(':');
                        var headerKey = splitHeader[0].Trim();
                        var headerValue = splitHeader[1].Trim();
                        header.Add(headerKey, headerValue);
                    }
                }
                return await next(context);
            }
        }
        public class ResponseHandlingFilter : IClientFilter
        {
            private readonly bool _verbose;

            public ResponseHandlingFilter(bool verbose)
            {
                _verbose = verbose;
            }
            public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
            {
                // capture request metadata
                var requestHeaders = context.CallOptions.Headers;

                var response = await next(context);
                var content = await response.GetResponseAs<string>();

                // request metadata
                if (_verbose)
                {
                    Console.WriteLine("Request headers to send:");
                    foreach (var requestHeader in requestHeaders)
                    {
                        Console.WriteLine($"{requestHeader.Key}: {requestHeader.Value}");
                    }
                    Console.WriteLine();
                }

                // response headers
                if (_verbose)
                {
                    Console.WriteLine("Response headers received:");
                    var responseHeaders = await response.ResponseHeadersAsync;
                    if (!responseHeaders.Any())
                    {
                        Console.WriteLine("(empty)");
                    }
                    else
                    {
                        foreach (var responseHeader in responseHeaders)
                        {
                            Console.WriteLine($"{responseHeader.Key}: {responseHeader.Value}");
                        }
                    }
                    Console.WriteLine();
                }

                // contents
                if (_verbose)
                {
                    Console.WriteLine("Response contents:");
                    Console.WriteLine("{");
                    Console.WriteLine($"  content: {content}");
                    Console.WriteLine("}");
                    Console.WriteLine();
                }

                // response trailers
                if (_verbose)
                {
                    Console.WriteLine("Response trailers received:");
                    var responseTrailers = response.GetTrailers();
                    if (!responseTrailers.Any())
                    {
                        Console.WriteLine("(empty)");
                    }
                    else
                    {
                        foreach (var responseTrailer in responseTrailers)
                        {
                            Console.WriteLine($"{responseTrailer.Key}: {responseTrailer.Value}");
                        }
                    }
                    Console.WriteLine();
                }

                return response;
            }
        }
    }
}
