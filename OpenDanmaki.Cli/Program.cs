using BiliApi;
using log4net;
using log4net.Config;
using QRCoder;
using System.Net;

namespace OpenDanmaki.Cli
{
    internal class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            Console.WriteLine("OpenDanmaki - Alpha test version");
            Console.WriteLine("Loading logger: log4net");
            if (File.Exists("log4net.config"))
            {
                XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
            }
            else
            {
                BasicConfigurator.Configure();
                log.Warn("Config file for log4net not exists, default logger loaded.");
            }
            log.Info("Logger loaded.");

            Config config;
            BiliApi.BiliSession bsession;
            BiliApi.Auth.QRLogin qr;

            if (File.Exists("config.json"))
            {
                config = new Config(File.ReadAllText("config.json"));
                log.Info("读取到配置文件，自动登录...");
                qr = new BiliApi.Auth.QRLogin(config.BiliCookie);
                if (!qr.LoggedIn)
                {
                    log.Info("正在连接B站认证服务...");
                    qr = new BiliApi.Auth.QRLogin();
                    {
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(new PayloadGenerator.Url(qr.QRToken.ScanUrl), QRCodeGenerator.ECCLevel.M);
                        AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);
                        Console.WriteLine(qrCode.GetGraphic(1));
                    }
                    log.Info("扫描上面的二维码，登录您的B站账号。");
                    qr.Login();
                    log.Info("登录成功");
                }
                else
                {
                    log.Info("登录成功");
                }
                bsession = new BiliApi.BiliSession(qr.Cookies);
                config.BiliCookie = SerializeCookie(qr.Cookies);
            }
            else
            {
                try
                {
                    config = new Config()
                    {
                        LocalHostname = "localhost",
                        LocalPort = 9753
                    };
                    log.Info("第一次使用 - 快速设置");
                    log.Info("正在连接B站认证服务...");
                    qr = new BiliApi.Auth.QRLogin();
                    {
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(new PayloadGenerator.Url(qr.QRToken.ScanUrl), QRCodeGenerator.ECCLevel.M);
                        AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);
                        Console.WriteLine(qrCode.GetGraphic(1));
                    }
                    log.Info("扫描上面的二维码，登录您的B站账号。");
                    var cookie = qr.Login();
                    log.Info("登录成功");
                    bsession = new BiliApi.BiliSession(cookie);
                    config.BiliCookie = SerializeCookie(cookie);
                    var userinfo = bsession.getCurrentUserData();
                    if (userinfo.liveroomid <= 0)
                    {
                        log.Info("要绑定到哪个直播间？输入直播间号。");
                        config.TargetRoomId = int.Parse(Console.ReadLine());
                    }
                    else
                    {
                        log.Info("将绑定到您的直播间："+userinfo.liveroomid);
                        config.TargetRoomId = userinfo.liveroomid;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("快速设置失败", ex);
                    log.Info("重启软件再次设置，或联系你的技术支持。");
                    return;
                }
            }
            OpenDanmaki od = new OpenDanmaki(config);
            od.StartAsync().Wait();
            config.BiliCookie = qr.Serilize();
            File.WriteAllText("config.json", config.Serilize());
            if (!File.Exists("./visual_assets/kboard/kboard.html"))
            {
                log.Warn("前端没有提供kboard.html文件");
            }
            else
            {
                Console.WriteLine("使用直播软件捕获以下地址：");
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("http://" + od.Server.Host + ":" + od.Server.Port + "/kboard.html");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
            }
            while (true) Thread.Sleep(10000);
        }

        public static string SerializeCookie(CookieCollection cookies)
        {
            List<string> cookieList = new List<string>();
            foreach (Cookie cookie in cookies)
            {
                cookieList.Add(cookie.Name + "=" + cookie.Value + ";");
            }
            return string.Join(" ", cookieList);
        }
    }
}