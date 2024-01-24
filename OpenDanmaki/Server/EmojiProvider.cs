using BiliveDanmakuAgent.Model;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace OpenDanmaki.Server
{
    public class EmojiProvider
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(EmojiProvider));
        public static void LoadEmojiIfNotExists(Danmaku dmkmsg)
        {
            if (dmkmsg.MsgType != DanmakuMsgType.Comment) return;
            var extra_jstr = dmkmsg.RawObject?["info"]?[0]?[15]?["extra"]?.ToString();
            if (extra_jstr is null) return;
            JObject extra = JObject.Parse(extra_jstr);
            foreach (JProperty emote in extra["emots"])
            {
                var emotename = emote.Name.Substring(1, emote.Name.Length - 2);
                if (!File.Exists("./visual_assets/emoji/" + emotename + ".png"))
                {
                    log.Debug("Preheating new emoji: " + emotename);
                    var bytes = AvatarProvider.Download(emote.Value.Value<string>("url")).Result;
                    File.WriteAllBytes("./visual_assets/emoji/" + emotename + ".png", bytes);
                }
            }
        }
    }
}
