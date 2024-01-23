using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace OpenDanmaki.Server
{
    public class ResourcesServer
    {
        HttpService service = new HttpService();

        public static List<Placeholder> Placeholders = new List<Placeholder>();

        public void Start(int port = 9753)
        {
            Placeholders.Add(new Placeholder
            {
                Name = "<<PLACEHOLDER_WEBSOCKET_URL>>",
                Value = "ws://localhost:" + port + "/msgpush"
            });

            service.Setup(new TouchSocketConfig()
            .SetListenIPHosts(port)
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

            service.Start();
        }
    }

    public struct Placeholder
    {
        public string Name;
        public string Value;
    }
}