using log4net;
using log4net.Config;
using BiliveDanmakuAgent;
using OpenDanmaki.Server;


namespace OpenDanmaki.Cli
{
    internal class Program
    {
        const string bilicookie = "buvid3=2E1148AD-ABC6-E0CF-37A7-9C82FC51C4FA40742infoc; i-wanna-go-back=-1; _uuid=A5D1A192-D104D-E15F-F335-7BF278949EA540928infoc; FEED_LIVE_VERSION=V8; rpdid=|(m)mk~u~u|0J'uY))JYukuk; header_theme_version=CLOSE; LIVE_BUVID=AUTO9216888353863270; buvid_fp_plain=undefined; b_ut=5; hit-new-style-dyn=1; hit-dyn-v2=1; dy_spec_agreed=1; b_nut=1695029494; buvid4=436331F3-85BC-E5CA-A9C4-1674DAEB99AF94655-023091817-ZJxtsW9ohCNsHMCIIXWPtA%3D%3D; enable_web_push=DISABLE; DedeUserID=23696210; DedeUserID__ckMd5=51fcc7bc4203c25e; fingerprint=b3ee44306f3c1f7a9f57b2d50851e9b2; CURRENT_FNVAL=4048; CURRENT_QUALITY=116; bp_article_offset_23696210=883478367681118213; SESSDATA=4135581f%2C1721318532%2C3f408%2A11CjDrLPC-7pETXG3gEWzMymgBt3EC_2o-zCtLi3iEOp1oqyAuM2FOlbactQY26x07QkgSVnpMUmttbnYxWl9OY2twR1J1VmRXcVktODVRbks5ZUZNRk82RXM3dGZycUFzZWxxdEM0bXBLbXkxc2ZkWjlSMXUzVnB6REUwT0RicTNWeDVDNXpZM1VBIIEC; bili_jct=9ef682ba0fd8006257acab9a9f8e18a1; bili_ticket=eyJhbGciOiJIUzI1NiIsImtpZCI6InMwMyIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3MDYyNTQxMDgsImlhdCI6MTcwNTk5NDg0OCwicGx0IjotMX0.6gqyh-A_fHyj9WVJd-WqC53f3oP410LgGVWCNYriwf0; bili_ticket_expires=1706254048; bsource=search_bing; b_lsid=45AD4DE4_18D358B5468; sid=8qz9qt66; home_feed_column=5; buvid_fp=b3ee44306f3c1f7a9f57b2d50851e9b2; bp_video_offset_23696210=889761814671261747; browser_resolution=1475-958; PVID=16";
        static void Main(string[] args)
        {
            OpenDanmaki od = new OpenDanmaki(404538, bilicookie);
            od.StartAsync().Wait();
            Console.WriteLine("OpenDanmaki - Alpha test version");
            Console.WriteLine("内部测试版本已装载");
            Console.WriteLine("http://" + od.Server.Host + ":" + od.Server.Port + "/kboard.html");
            while (true) Thread.Sleep(1000);
        }
    }
}