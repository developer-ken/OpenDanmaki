using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki
{
    public interface IPlugin
    {
        public string PluginName { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }

        public void OnPluginLoad(OpenDanmaki od_base);
    }
}
