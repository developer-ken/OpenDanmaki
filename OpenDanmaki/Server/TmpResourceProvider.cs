using BiliApi;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki.Server
{
    public class TmpResourceProvider
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(TmpResourceProvider));
        public Dictionary<int, CachedItem> FileCache = new Dictionary<int, CachedItem>();
        private int selfinc = 0;
        private string upf;

        public TmpResourceProvider(string url_prefix)
        {
            upf = url_prefix;
        }

        public async Task<int> LoadUrl(string url, string reference = "https://www.bilibili.com/", bool onetime = true)
        {
            log.Debug("Downloading data from  " + url);
            byte[] result;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = BiliSession.USER_AGENT;
            request.Referer = reference;
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            Stream myResponseStream = response.GetResponseStream();
            BinaryReader streamReader = new BinaryReader(myResponseStream);
            result = streamReader.ReadBytes((int)response.ContentLength);
            streamReader.Close();
            myResponseStream.Close();
            log.Debug("Downloaded.");
            var cached = new CachedItem
            {
                Data = result,
                XMineType = response.ContentType,
                OnetimeUseOnly = onetime
            };
            selfinc++;
            FileCache.Add(selfinc, cached);
            return selfinc;
        }

        public CachedItem GetCachedItem(int id)
        {
            if (FileCache.ContainsKey(id))
            {
                var item = FileCache[id];
                if (item.OnetimeUseOnly)
                {
                    FileCache.Remove(id);
                }
                return item;
            }
            else
            {
                throw new Exception("No such cached item.");
            }
        }

        public string AttachedFile(string url, string reference = "https://www.bilibili.com/", bool destroy_after_use = true)
        {
            int id = LoadUrl(url, reference, destroy_after_use).Result;
            return upf + id;
        }
    }

    public struct CachedItem
    {
        public byte[] Data;
        public string XMineType;
        public bool OnetimeUseOnly;
    }
}
