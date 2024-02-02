using BiliveDanmakuAgent.Model;
using log4net;
using Newtonsoft.Json.Linq;
using OpenDanmaki.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace OpenDanmaki.Server
{
    public class WSMessagePush
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(WSMessagePush));
        ResourcesServer Server;
        public event Action<RawJsonEventArgs> MessagePrepush;

        public WSMessagePush(ResourcesServer server)
        {
            Server = server;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="danmaku">弹幕</param>
        /// <param name="priority">优先级(持续时间)</param>
        /// <param name="tags">标签</param>
        public async Task PushStdMessageAsync(Danmaku danmaku, int priority = 0, List<string> tags = null)
        {
            await HttpHandler.avatar.AvatarPreheatAsync(danmaku.UserID, danmaku.AvatarUrl);
            EmojiProvider.LoadEmojiIfNotExists(danmaku);

            JObject base_obj = new JObject();
            if (priority > 0)
            {
                base_obj.Add("type", "pinned");
            }
            else
            {
                base_obj.Add("type", "normal");
            }
            base_obj.Add("name", danmaku.UserName);
            base_obj.Add("uid", danmaku.UserID);
            base_obj.Add("content", HandleEmoji(danmaku));
            base_obj.Add("avatar", "http://" + Server.Host + ":" + Server.Port + "/imageservice/avatar/" + danmaku.UserID);
            base_obj.Add("priority", priority);
            base_obj.Add("guard_level", danmaku.UserGuardLevel);
            if (tags is null || tags.Count() == 0)
            {
                base_obj.Add("tags", null);
            }
            else
            {
                JArray jarr = new JArray();
                foreach (var item in tags)
                {
                    jarr.Add(item);
                }
                base_obj.Add("tags", jarr);
            }

            {
                JObject medal = new JObject();
                if (danmaku.UserMedal != null)
                {
                    medal.Add("title", danmaku.UserMedal.Name);
                    medal.Add("level", danmaku.UserMedal.Level);
                    medal.Add("tuid", danmaku.UserMedal.TargetId);
                    medal.Add("tname", danmaku.UserMedal.TargetName);
                }
                base_obj.Add("medal", medal);
            }

            await PushJsonToClientAsync(base_obj);
        }

        public async Task PushGiftAsync(Danmaku danmaku, List<string> tags = null)
        {
            JObject base_obj = new JObject();
            base_obj.Add("type", "gift");
            base_obj.Add("name", danmaku.UserName);
            base_obj.Add("uid", danmaku.UserID);
            base_obj.Add("avatar", "http://" + Server.Host + ":" + Server.Port + "/imageservice/avatar/" + danmaku.UserID);
            base_obj.Add("gift_name", danmaku.GiftName);
            base_obj.Add("gift_count", danmaku.GiftCount);
            base_obj.Add("gift_cost", danmaku.Price);
            base_obj.Add("gift_is_gold", danmaku.GiftGoldcoin);
            if (danmaku.GiftId is not null)
                base_obj.Add("img_url",
                    OpenDanmaki.instance.GiftResourcesProvider.GetGiftPicture((int)danmaku.GiftId));

            if (tags is null || tags.Count() == 0)
            {
                base_obj.Add("tags", null);
            }
            else
            {
                JArray jarr = new JArray();
                foreach (var item in tags)
                {
                    jarr.Add(item);
                }
                base_obj.Add("tags", jarr);
            }

            {
                JObject medal = new JObject();
                if (danmaku.UserMedal != null)
                {
                    medal.Add("title", danmaku.UserMedal.Name);
                    medal.Add("level", danmaku.UserMedal.Level);
                    medal.Add("tuid", danmaku.UserMedal.TargetId);
                    medal.Add("tname", danmaku.UserMedal.TargetName);
                }
                base_obj.Add("medal", medal);
            }
            await PushJsonToClientAsync(base_obj);
        }

        public async Task PushCrewAsync(Danmaku danmaku, List<string> tags = null)
        {
            JObject base_obj = new JObject();
            base_obj.Add("type", "crew");
            base_obj.Add("name", danmaku.UserName);
            base_obj.Add("uid", danmaku.UserID);
            base_obj.Add("avatar", "http://" + Server.Host + ":" + Server.Port + "/imageservice/avatar/" + danmaku.UserID);
            base_obj.Add("ctype", danmaku.UserGuardLevel);
            base_obj.Add("gift_count", danmaku.GiftCount);
            base_obj.Add("len", danmaku.GiftCount);

            if (tags is null || tags.Count() == 0)
            {
                base_obj.Add("tags", null);
            }
            else
            {
                JArray jarr = new JArray();
                foreach (var item in tags)
                {
                    jarr.Add(item);
                }
                base_obj.Add("tags", jarr);
            }

            {
                JObject medal = new JObject();
                if (danmaku.UserMedal != null)
                {
                    medal.Add("title", danmaku.UserMedal.Name);
                    medal.Add("level", danmaku.UserMedal.Level);
                    medal.Add("tuid", danmaku.UserMedal.TargetId);
                    medal.Add("tname", danmaku.UserMedal.TargetName);
                }
                base_obj.Add("medal", medal);
            }
            await PushJsonToClientAsync(base_obj);
        }

        public async Task PushJsonToClientAsync(JObject json)
        {
            var arg = new RawJsonEventArgs(json);
            MessagePrepush?.Invoke(arg);
            if (arg.Drop)
            {
                log.Info("Plugin dropped a message.");
            }
            int cntsent = 0;
            var socketClients = Server.service.GetClients();
            foreach (var item in socketClients)
            {
                if (item.Protocol == Protocol.WebSocket)//先判断是不是websocket协议
                {
                    await item.SendWithWSAsync(arg.RawJson.ToString(Newtonsoft.Json.Formatting.None));
                    cntsent++;
                }
            }
            if (cntsent == 0)
            {
                log.Warn("No client connected! Message dropped.");
                log.Warn("Check client connection.");
                log.Warn("前端未运行或无法通信，请检查！");
            }
        }

        public string HandleEmoji(Danmaku dmk)
        {
            string text = dmk.CommentText;
            try
            {
                var dataurl = dmk.RawObject?["info"]?[0]?[13]?["url"]?.ToString();
                if (dataurl is not null)
                {
                    var emotename = dmk.RawObject?["info"]?[0]?[13]?["emoticon_unique"]?.ToString();
                    emotename = emotename.Replace("[", "").Replace("]", "");
                    if (!File.Exists("./visual_assets/emoji/" + emotename + ".png"))
                    {
                        log.Debug("Preheating new emoji: " + emotename);
                        var bytes = AvatarProvider.Download(dataurl).Result;
                        File.WriteAllBytes("./visual_assets/emoji/" + emotename + ".png", bytes);
                    }
                    text = "[img:/emoji/" + emotename + ".png]";
                    return text;
                }
            }
            catch { }
            var matches = Regex.Matches(text, @"\[([A-Za-z0-9\u4e00-\u9fa5\-_]+)\]");
            foreach (Match item in matches)
            {
                if (File.Exists("./visual_assets/emoji/" + item.Groups[1].Value + ".png"))
                    text = text.Replace(item.Value, "[emoj:/emoji/" + item.Groups[1].Value + ".png]");
            }
            return text;
        }
    }
}