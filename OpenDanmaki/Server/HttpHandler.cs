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
        public async Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            string fpath = Path.Combine("./visual_assets", e.Context.Request.RelativeURL.Substring(1));
            if (File.Exists(fpath))
            {
                var contenttype = HttpTools.GetContentTypeFromExtension(Path.GetExtension(fpath));
                if (contenttype.Contains("text") || contenttype.Contains("javascript"))
                {
                    e.Context.Response.ContentType = contenttype;
                    e.Context.Response
                        .SetStatus()
                        .SetContent(FillPlaceholders(File.ReadAllText(fpath),ResourcesServer.Placeholders));
                }
                else
                {
                    e.Context.Response.ContentType = contenttype;
                    e.Context.Response
                        .SetStatus()
                        .SetContent(File.ReadAllBytes(fpath));
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
