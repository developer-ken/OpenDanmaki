using log4net;

namespace OpenDanmaki
{
    public interface IPlugin
    {
        public string PluginName { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }

        public void OnPluginLoad(OpenDanmaki od_base, ILog logger);
    }
}
