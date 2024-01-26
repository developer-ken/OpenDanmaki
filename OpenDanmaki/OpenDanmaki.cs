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
        public static OpenDanmaki instance;

        public ResourcesServer Server;
        public WSMessagePush WSMPush;
        public DanmakuApi BiliDanmaku;
        public PluginLoader Pluginloader;
        public TmpResourceProvider TmpResourceProvider;

        /// <summary>
        /// 在弹幕事件(包含sc、礼物等)接收后、被处理前触发
        /// </summary>
        public event Action<DanmakuEventArgs> DanmakuPreprocess;

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

        /// <summary>
        /// 在一段Json发送到前端前触发
        /// </summary>

        public event Action<RawJsonEventArgs> MessagePrepush;

        private static readonly ILog logger = LogManager.GetLogger(typeof(OpenDanmaki));

        public OpenDanmaki(int liveroomid, string logincookie = "", string host = "localhost", int port = 9753)
        {
            instance = this;
            Server = new ResourcesServer(host, port);
            WSMPush = new WSMessagePush(Server);
            BiliDanmaku = new DanmakuApi(liveroomid, logincookie);
            Pluginloader = new PluginLoader(this);
            TmpResourceProvider = new TmpResourceProvider("http://" + host + ":" + port + "/attachments/");
            BiliDanmaku.DanmakuMsgReceivedEvent += BiliDanmaku_DanmakuMsgReceivedEvent;
            BiliDanmaku.CommentReceived += BiliDanmaku_CommentReceived;
            BiliDanmaku.Superchat += BiliDanmaku_Superchat;
            BiliDanmaku.Gift += BiliDanmaku_Gift;
            BiliDanmaku.GuardBuy += BiliDanmaku_GuardBuy;
        }
        

        public OpenDanmaki(Config config) : this(config.TargetRoomId, config.BiliCookie, config.LocalHostname, config.LocalPort) { }

        private void BiliDanmaku_DanmakuMsgReceivedEvent(object sender, DanmakuReceivedEventArgs e)
        {
            var args = new DanmakuEventArgs(e.Danmaku);
            DanmakuPreprocess?.Invoke(args);
            if (args.Drop) e.Danmaku.MsgType = DanmakuMsgType.Unknown; // Drop the message
        }

        private void BiliDanmaku_Gift(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            var args = new DanmakuEventArgs(e.Danmaku);
            GiftPreprocess?.Invoke(args);
            if (args.Drop)
            {
                logger.Debug("[DROP]" + e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [gift]" + e.Danmaku.GiftName + " x" + e.Danmaku.GiftCount);
                return;
            }
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [gift]" + e.Danmaku.GiftName + " x" + e.Danmaku.GiftCount);
            WSMPush.PushGiftAsync(args.DanmakuObj, args.BandageImgUrls);
        }

        private void BiliDanmaku_Superchat(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            var args = new DanmakuEventArgs(e.Danmaku);
            SuperchatPreprocess?.Invoke(args);
            if (args.Drop)
            {
                logger.Debug("[DROP]" + e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [superchat]" + e.Danmaku.CommentText);
                return;
            }
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [superchat]" + e.Danmaku.CommentText);
            WSMPush.PushStdMessageAsync(args.DanmakuObj, e.Danmaku.SCKeepTime, args.BandageImgUrls);
        }

        private void BiliDanmaku_GuardBuy(object sender, DanmakuReceivedEventArgs e)
        {
            var args = new DanmakuEventArgs(e.Danmaku);
            GiftPreprocess?.Invoke(args);
            if (args.Drop)
            {
                logger.Debug("[DROP]" + e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [crew] Type" + e.Danmaku.UserGuardLevel + " x" + e.Danmaku.GiftCount);
                return;
            }
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [crew] Type" + e.Danmaku.UserGuardLevel + " x" + e.Danmaku.GiftCount);
            WSMPush.PushCrewAsync(args.DanmakuObj, args.BandageImgUrls);
        }

        private void BiliDanmaku_CommentReceived(object sender, BiliveDanmakuAgent.Model.DanmakuReceivedEventArgs e)
        {
            var args = new DanmakuEventArgs(e.Danmaku);
            CommentPreprocess?.Invoke(args);
            if (args.Drop)
            {
                logger.Debug("[DROP]" + e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [chat]" + e.Danmaku.CommentText);
                return;
            }
            logger.Debug(e.Danmaku.UserName + "#" + e.Danmaku.UserID + ": [chat]" + e.Danmaku.CommentText);
            WSMPush.PushStdMessageAsync(args.DanmakuObj, e.Danmaku.SCKeepTime, args.BandageImgUrls);
        }

        public async Task StartAsync()
        {
            await Server.StartAsync();
            await BiliDanmaku.ConnectAsync();
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("OpenDanmaki " + ver.ToString() + " loaded.");
            logger.Info("Start loading plugins...");
            Directory.CreateDirectory("plugins");
            Directory.GetFiles("plugins").ToList().ForEach(x =>
            {
                if (x.ToLower().EndsWith(".odp.dll"))
                    Pluginloader.LoadPlugin(x);
            });
            logger.Info("Finished loading plugins.");
        }
    }
}
