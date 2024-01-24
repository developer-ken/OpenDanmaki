using log4net;
using OpenDanmaki;
using OpenDanmaki.Model;

/*
 * 这是一个示例插件，展示如何通过后端代码修改弹幕内容。
 * 
 * 此插件在舰长弹幕上附加一个特殊徽章。
 * 请注意，徽章大小、位置、加载与否是前端决定的。
 * 
 * 为避免前端跨域问题，在使用网络资源时请使用临时资源提供器TmpResourceProvider.AttachedFile。
 * 此工具将在后端预加载需要的文件，然后返回一个主机URL。
 * destroy_after_use为true时，url访问一次后资源销毁，反之则保留在内存中供以后使用。
 */

namespace PluginExample
{
    public class Plugin1 : IPlugin
    {
        public string PluginName { get; set; } = "Plugin1";
        public string Author { get; set; } = "Author1";
        public Version Version { get; set; } = new Version(1, 0, 0, 0);

        private ILog logger;

        string crewtagurl;

        public void OnPluginLoad(OpenDanmaki.OpenDanmaki od_base, ILog logger)
        {
            od_base.CommentPreprocess += Od_base_DanmakuReceived;
            this.logger = logger;

            //加载舰长标识。此工具将在后端预加载需要的文件，然后返回一个指向资源的主机URL。
            crewtagurl =
                od_base.TmpResourceProvider.AttachedFile(
                    "https://i0.hdslb.com/bfs/live/143f5ec3003b4080d1b5f817a9efdca46d631945.png",
                    destroy_after_use: false
                );
        }

        private void Od_base_DanmakuReceived(DanmakuEventArgs e)
        {
            if (e.DanmakuObj.UserGuardLevel <= 0) //不是舰长
            {
                //e.Drop = true; //丢弃这条弹幕
                //logger.Debug("Example plugin droped a danmaku! ");
            }
            else
            {
                e.BandageImgUrls.Add(crewtagurl); //添加舰长标识
                //弹幕后面加字
                //e.DanmakuObj.CommentText += "[ExamplePlugin]";
                logger.Debug("Example plugin modified a danmaku! ");
            }
        }
    }
}