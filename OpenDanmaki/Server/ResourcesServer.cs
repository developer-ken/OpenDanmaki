using System.Reflection;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace OpenDanmaki.Server
{
    public class ResourcesServer
    {
        public HttpService service = new HttpService();

        public int Port { get; private set; }
        public string Host { get; private set; }

        public static List<Placeholder> Placeholders = new List<Placeholder>();

        public ResourcesServer(string host = "localhost", int port = 9753)
        {
            Host = host;
            Port = port;
        }

        public async Task StartAsync()
        {
            Placeholders.Add(new Placeholder
            {
                Name = "<<PLACEHOLDER_WEBSOCKET_URL>>",
                Value = "ws://" + Host + ":" + Port + "/msgpush"
            });
            Placeholders.Add(new Placeholder
            {
                Name = "<<PLACEHOLDER_HOST>>",
                Value = Host
            });
            Placeholders.Add(new Placeholder
            {
                Name = "<<PLACEHOLDER_PORT>>",
                Value = Port.ToString()
            });

            service.Setup(new TouchSocketConfig()
            .SetListenIPHosts(Port)
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                //以下即是插件
                a.UseWebSocket()//WebSocket
                    .SetWSUrl("/msgpush")
                    .UseAutoPong();//自动回应ping
                a.Add<HttpHandler>();
                a.UseDefaultHttpServicePlugin();
            }));

            await service.StartAsync();
        }
    }

    public struct Placeholder
    {
        public string Name;
        public string Value;
    }
}