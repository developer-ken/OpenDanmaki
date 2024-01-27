using BiliApi;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki.Server
{
    public class AvatarProvider
    {
        public Dictionary<long, byte[]> AvatarCache = new Dictionary<long, byte[]>();
        public Dictionary<long, object> AvatarLock = new Dictionary<long, object>();

        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(HttpHandler));

        public byte[] GetAvatar(long uid)
        {
            lock (Lock(uid))
            {
                if (AvatarCache.ContainsKey(uid))
                {
                    var cache = AvatarCache[uid];
                    if(!OpenDanmaki.Config.CacheAvatar) AvatarCache.Remove(uid);
                    return cache;
                }
                else
                {
                    log.Warn("Cold avatar detected for " + uid);
                    log.Warn("Have to fetch on-time. This should never happen!");
                    BiliUser user = BiliUser.getUser(uid);
                    byte[] avatar = Download(user.face).Result;
                    AvatarCache.Add(uid, avatar);
                    return avatar;
                }
            }
        }

        public async Task AvatarPreheatAsync(long uid, string uri)
        {
            await Task.Run(() =>
            {

                lock (Lock(uid))
                {
                    if (!AvatarCache.ContainsKey(uid))
                    {
                        try
                        {
                            AvatarCache.Add(uid, Download(uri).Result);
                            log.Debug("Preheated avatar for " + uid);
                        }
                        catch (Exception err)
                        {
                            log.Error("Error preheating avatar for " + uid, err);
                        }
                    }
                }
            });
        }

        private object Lock(long uid)
        {
            lock (AvatarLock)
            {
                if (!AvatarLock.ContainsKey(uid))
                {
                    AvatarLock.Add(uid, new object());
                }
                return AvatarLock[uid];
            }
        }

        public static async Task<byte[]> Download(string url, string reference = "https://www.bilibili.com/")
        {
            if (url is null)
            {
                throw new Exception("Downloading from a null url!");
            }
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
            return result;
        }
    }
}
