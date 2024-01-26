using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki
{
    public class Config
    {
        public int TargetRoomId { get; set; }
        public string BiliCookie { get; set; }
        public string LocalHostname { get; set; }
        public int LocalPort { get; set; }
        
        public Config()
        {

        }

        public Config(string json)
        {
            JObject j = JObject.Parse(json);
            TargetRoomId = j.Value<int>("TargetRoomId");
            BiliCookie = j.Value<string>("BiliCookie");
            LocalHostname = j.Value<string>("LocalHostname");
            LocalPort = j.Value<int>("LocalPort");
        }

        public string Serilize()
        {
            JObject j = new JObject();
            j.Add("TargetRoomId", TargetRoomId);
            j.Add("BiliCookie", BiliCookie);
            j.Add("LocalHostname", LocalHostname);
            j.Add("LocalPort", LocalPort);
            return j.ToString();
        }
    }
}
