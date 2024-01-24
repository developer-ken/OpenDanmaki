using BiliveDanmakuAgent;
using BiliveDanmakuAgent.Model;
using log4net;
using log4net.Repository.Hierarchy;
using OpenDanmaki.Model;
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

        /// <summary>
        /// 在弹幕事件(包含sc、礼物等)接收后、被处理前触发
        /// </summary>
        public event Action<Danmaku> DanmakuPreprocess;

        /// <summary>
        /// 弹幕礼物接收后、被处理前触发
        /// </summary>
        public event Action<DanmakuEventArgs> GiftPreprocess;

        /// <summary>
        /// 醒目留言接收后、被处理前触发
        /// </summary>
        public event Action<DanmakuEventArgs> SuperchatPreprocess;

        /// <summary>
        /// 上舰事件接收后、被处理前触发
        /// </summary>
        public event Action<DanmakuEventArgs> GuardEventPreprocess;

        /// <summary>
        /// 弹幕消息接收后、被处理前触发
        /// </summary>
        public event Action<DanmakuEventArgs> CommentPreprocess;

        private static readonly ILog logger = LogManager.GetLogger(typeof(OpenDanmaki));

        public OpenDanmaki(int liveroomid, string logincookie = "", string host = "localhost", int port = 9753)
        {
            Server = new ResourcesServer(host, port);
            WSMPush = new WSMessagePush(Server);
            BiliDanmaku = new DanmakuApi(liveroomid, logincookie);
            BiliDanmaku.DanmakuMsgReceivedEvent += BiliDanmaku_DanmakuMsgReceivedEvent;
            BiliDanmaku.CommentReceived += BiliDanmaku_CommentReceived;
            BiliDanmaku.Superchat += BiliDanmaku_Superchat;
            BiliDanmaku.Gift += BiliDanmaku_Gift;
        }

        private void BiliDanmaku_DanmakuMsgReceivedEvent(object sender, DanmakuReceivedEventArgs e)
        {
            DanmakuPreprocess?.Invoke(e.Danmaku);
        }

        private void BiliDanmaku_Gift(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            var args = new DanmakuEventArgs(e.Danmaku);
            GiftPreprocess?.Invoke(args);
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [gift]" + e.Danmaku.GiftName + " x" + e.Danmaku.GiftCount);
            WSMPush.PushGift(args.DanmakuObj, args.BandageImgUrls);
        }

        private void BiliDanmaku_Superchat(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            var args = new DanmakuEventArgs(e.Danmaku);
            SuperchatPreprocess?.Invoke(args);
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [superchat]" + e.Danmaku.CommentText);
            WSMPush.PushStdMessage(args.DanmakuObj, e.Danmaku.SCKeepTime, args.BandageImgUrls);
        }

        private void BiliDanmaku_CommentReceived(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            var args = new DanmakuEventArgs(e.Danmaku);
            CommentPreprocess?.Invoke(args);
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [chat]" + e.Danmaku.CommentText);
            WSMPush.PushStdMessage(args.DanmakuObj, e.Danmaku.SCKeepTime, args.BandageImgUrls);
        }

        public async Task StartAsync()
        {
            await Server.StartAsync();
            await BiliDanmaku.ConnectAsync();
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("OpenDanmaki " + ver.ToString() + " loaded.");
        }
    }
}
