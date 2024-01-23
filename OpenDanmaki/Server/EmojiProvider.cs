using BiliveDanmakuAgent.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki.Server
{
    public class EmojiProvider
    {
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
                    var bytes = AvatarProvider.Download(emote.Value.Value<string>("url")).Result;
                    File.WriteAllBytes("./visual_assets/emoji/" + emotename + ".png", bytes);
                }
            }
        }
    }
}
