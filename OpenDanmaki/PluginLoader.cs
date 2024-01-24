using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki
{
    public class PluginLoader
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PluginLoader));
        public OpenDanmaki ODBase { get; internal set; }

        public List<IPlugin> Plugins { get; internal set; }

        public PluginLoader(OpenDanmaki odbase)
        {
            Plugins = new List<IPlugin>();
            ODBase = odbase;
        }

        public void LoadPlugin(string path)
        {
            try
            {
                logger.Info("Loading assembly from file: " + path);
                Assembly ass = Assembly.LoadFrom(path);
                var wormMain = ass.GetTypes().FirstOrDefault(m => m.GetInterface(typeof(IPlugin).Name) != null);
                ILog pluginlogger = LogManager.GetLogger(wormMain);
                var tmpObj = (IPlugin)Activator.CreateInstance(wormMain);
                try
                {
                    logger.Info("Loading " + tmpObj.PluginName + " version " + tmpObj.Version.ToString()
                        + " by " + tmpObj.Author);
                    tmpObj.OnPluginLoad(ODBase, pluginlogger);
                    Plugins.Add(tmpObj);
                }
                catch (Exception ex)
                {
                    logger.Error("Faild loading " + tmpObj.PluginName + " version " + tmpObj.Version.ToString()
                        + " by " + tmpObj.Author, ex);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed loading \"" + path + "\"", ex);
            }
        }
    }
}
