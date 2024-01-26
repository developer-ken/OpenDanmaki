using log4net;

namespace OpenDanmaki
{
    public interface IPlugin
    {
        public string PluginName { get; }
        public string Author { get; }
        public Version Version { get; }

        public void OnPluginLoad(OpenDanmaki od_base, ILog logger);
    }
}
