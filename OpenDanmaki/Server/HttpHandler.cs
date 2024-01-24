using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;

namespace OpenDanmaki.Server
{
    public class HttpHandler : PluginBase, IHttpPlugin<IHttpSocketClient>
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(HttpHandler));
        public static AvatarProvider avatar = new AvatarProvider();
        public async Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            //头像服务
            if (e.Context.Request.RelativeURL.StartsWith("/imageservice/avatar/"))
            {
                try
                {
                    long uid = long.Parse(e.Context.Request.RelativeURL.Substring("/imageservice/avatar/".Length));
                    e.Context.Response.ContentType = "image/jpeg";
                    e.Context.Response.SetContent(avatar.GetAvatar(uid));
                    await e.Context.Response.AnswerAsync();
                    return;
                }catch(Exception ex)
                {
                    log.Error("Failed serving avatar!",ex);
                }
            }
            //预抓取文件
            if (e.Context.Request.RelativeURL.StartsWith("/attachments/"))
            {
                int id = int.Parse(e.Context.Request.RelativeURL.Substring("/attachments/".Length));
                var item = OpenDanmaki.instance.TmpResourceProvider.GetCachedItem(id);
                e.Context.Response.ContentType = item.XMineType;
                e.Context.Response.SetContent(item.Data);
                await e.Context.Response.AnswerAsync();
                return;
            }
            //静态文件
            {
                string fpath1 = Path.Combine("./visual_assets", e.Context.Request.RelativeURL.Substring(1));
                string fpath2 = Path.Combine("./visual_assets/kboard", e.Context.Request.RelativeURL.Substring(1));
                if (File.Exists(fpath1))
                {
                    var contenttype = HttpTools.GetContentTypeFromExtension(Path.GetExtension(fpath1));
                    if (contenttype.Contains("text") || contenttype.Contains("javascript"))
                    {
                        e.Context.Response.ContentType = contenttype;
                        e.Context.Response
                            .SetStatus()
                            .SetContent(FillPlaceholders(File.ReadAllText(fpath1), ResourcesServer.Placeholders));
                    }
                    else
                    {
                        e.Context.Response.ContentType = contenttype;
                        e.Context.Response
                            .SetStatus()
                            .SetContent(File.ReadAllBytes(fpath1));
                    }
                }
                else
                if (File.Exists(fpath2))
                {
                    var contenttype = HttpTools.GetContentTypeFromExtension(Path.GetExtension(fpath2));
                    if (contenttype.Contains("text") || contenttype.Contains("javascript"))
                    {
                        e.Context.Response.ContentType = contenttype;
                        e.Context.Response
                            .SetStatus()
                            .SetContent(FillPlaceholders(File.ReadAllText(fpath2), ResourcesServer.Placeholders));
                    }
                    else
                    {
                        e.Context.Response.ContentType = contenttype;
                        e.Context.Response
                            .SetStatus()
                            .SetContent(File.ReadAllBytes(fpath2));
                    }
                }
                else
                {
                    e.Context.Response.StatusCode = 404;
                    e.Context.Response.ContentType = "text/html";
                    e.Context.Response.SetContent(Encoding.UTF8.GetBytes("<title>OpenDanmaki - 404 Not Found</title>Missing resource here."));
                }
                await e.Context.Response.AnswerAsync();
            }
        }

        public string FillPlaceholders(string text, List<Placeholder> placeholders)
        {
            foreach(var ph in placeholders)
            {
                text = text.Replace(ph.Name, ph.Value);
            }
            return text;
        }
    }
}
