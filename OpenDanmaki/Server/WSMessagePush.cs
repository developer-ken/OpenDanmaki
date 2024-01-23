using BiliveDanmakuAgent.Model;
using Newtonsoft.Json.Linq;
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
        ResourcesServer Server;

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
        public void PushStdMessage(Danmaku danmaku, int priority = 0, List<string> tags = null)
        {
            HttpHandler.avatar.AvatarPreheatAsync(danmaku.UserID, danmaku.AvatarUrl).Wait();
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
            base_obj.Add("content", HandleEmoji(danmaku.CommentText));
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
                    jarr.Add("http://" + Server.Host + ":" + Server.Port + "/imageservice/tags/" + item);
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

            var socketClients = Server.service.GetClients();
            foreach (var item in socketClients)
            {
                if (item.Protocol == Protocol.WebSocket)//先判断是不是websocket协议
                {
                    item.SendWithWS(base_obj.ToString(Newtonsoft.Json.Formatting.None));
                }
            }
        }

        public void PushGift(Danmaku danmaku, List<string> tags = null)
        {
            JObject base_obj = new JObject();
            base_obj.Add("type", "gift");
            base_obj.Add("name", danmaku.UserName);
            base_obj.Add("avatar", "http://" + Server.Host + ":" + Server.Port + "/imageservice/avatar/" + danmaku.UserID);
            base_obj.Add("gift_name", danmaku.GiftName);
            base_obj.Add("gift_count", danmaku.GiftCount);
            base_obj.Add("gift_cost", danmaku.Price);
            base_obj.Add("gift_is_gold", danmaku.GiftGoldcoin);

            if (tags is null || tags.Count() == 0)
            {
                base_obj.Add("tags", null);
            }
            else
            {
                JArray jarr = new JArray();
                foreach (var item in tags)
                {
                    jarr.Add("http://" + Server.Host + ":" + Server.Port + "/imageservice/tags/" + item);
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

            var socketClients = Server.service.GetClients();
            foreach (var item in socketClients)
            {
                if (item.Protocol == Protocol.WebSocket)//先判断是不是websocket协议
                {
                    item.SendWithWS(base_obj.ToString(Newtonsoft.Json.Formatting.None));
                }
            }
        }

        public string HandleEmoji(string text)
        {
            var matches = Regex.Matches(text, @"\[(.+)\]");
            foreach (Match item in matches)
            {
                if (File.Exists("./visual_assets/emoji/" + item.Groups[1].Value + ".png"))
                    text = text.Replace(item.Value, "[img:/emoji/" + item.Groups[1].Value + ".png]");
            }
            return text;
        }
    }
}