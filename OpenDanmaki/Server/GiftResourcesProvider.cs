using BiliApi;
using BiliveDanmakuAgent.Model;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki.Server
{
    public class GiftResourcesProvider
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(GiftResourcesProvider));
        Dictionary<int, string> resourcelist = new Dictionary<int, string>();
        string urlprefix;

        public GiftResourcesProvider(string prefix)
        {
            urlprefix = prefix;
        }

        public int LoadGiftResourcesList(Config conf)
        {
            var jstr = BiliSession._get("https://api.live.bilibili.com/xlive/web-room/v1/giftPanel/roomGiftConfig?platform=pc&room_id=" + conf.TargetRoomId);
            JObject jb = JObject.Parse(jstr);

            foreach (JToken j in jb["data"]["global_gift"]["list"])
            {
                var id = j["id"].Value<int>();
                var png = j["webp"].Value<string>();
                if (resourcelist.ContainsKey(id))
                {
                    continue;
                }
                resourcelist.Add(id, png);
            }
            foreach (JToken j in jb["data"]["list"])
            {
                var id = j["id"].Value<int>();
                var png = j["webp"].Value<string>();
                if (resourcelist.ContainsKey(id))
                {
                    continue;
                }
                resourcelist.Add(id, png);
            }
            return resourcelist.Count();
        }

        public string GetGiftPicture(int id)
        {
            if (resourcelist.ContainsKey(id))
            {
                var url = resourcelist[id];
                var target = "gifts/" + id.ToString() + "." + url.Split('.').Last();
                if (!File.Exists("./visual_assets/" + target))
                {
                    var pic = AvatarProvider.Download(url).Result;
                    File.WriteAllBytes("./visual_assets/" + target, pic);
                }
                return urlprefix + target;
            }
            else
            {
                return null;
            }
        }
    }
}
