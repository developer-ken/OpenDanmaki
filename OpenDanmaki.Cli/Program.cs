using OpenDanmaki.Server;

namespace OpenDanmaki.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ResourcesServer server = new ResourcesServer();
            server.Start();
            while (true) Thread.Sleep(1000);
        }
    }
}