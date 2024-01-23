using BiliveDanmakuAgent;
using log4net;
using log4net.Repository.Hierarchy;
using OpenDanmaki.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki
{
    public class OpenDanmaki
    {
        public ResourcesServer Server;
        public WSMessagePush WSMPush;
        public DanmakuApi BiliDanmaku;

        private static readonly ILog logger = LogManager.GetLogger(typeof(OpenDanmaki));

        public OpenDanmaki(int liveroomid, string logincookie = "", string host = "localhost", int port = 9753)
        {
            Server = new ResourcesServer(host, port);
            WSMPush = new WSMessagePush(Server);
            BiliDanmaku = new DanmakuApi(liveroomid, logincookie);
            BiliDanmaku.CommentReceived += BiliDanmaku_CommentReceived;
            BiliDanmaku.Superchat += BiliDanmaku_Superchat;
            BiliDanmaku.Gift += BiliDanmaku_Gift;
        }

        private void BiliDanmaku_Gift(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [gift]" + e.Danmaku.GiftName + " x" + e.Danmaku.GiftCount);
            WSMPush.PushGift(e.Danmaku);
        }

        private void BiliDanmaku_Superchat(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [superchat]" + e.Danmaku.CommentText);
            WSMPush.PushStdMessage(e.Danmaku, e.Danmaku.SCKeepTime);
        }

        private void BiliDanmaku_CommentReceived(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [chat]" + e.Danmaku.CommentText);
            WSMPush.PushStdMessage(e.Danmaku);
        }

        public async Task StartAsync()
        {
            await Server.StartAsync();
            await BiliDanmaku.ConnectAsync();
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("OpenDanmaki " + ver.ToString() + " loaded.");
        }
    }
}
