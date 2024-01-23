using BiliApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki.Server
{
    public class AvatarProvider
    {
        public Dictionary<long, byte[]> AvatarCache = new Dictionary<long, byte[]>();
        public Dictionary<long, object> AvatarLock = new Dictionary<long, object>();

        public byte[] GetAvatar(long uid)
        {
            lock (Lock(uid))
            {
                if (AvatarCache.ContainsKey(uid))
                {
                    return AvatarCache[uid];
                }
                else
                {
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
                        AvatarCache.Add(uid, Download(uri).Result);
                    }
                }
            });
        }

        private object Lock(long uid)
        {
            if (!AvatarLock.ContainsKey(uid))
            {
                AvatarLock.Add(uid, new object());
            }
            return AvatarLock[uid];
        }

        public static async Task<byte[]> Download(string url, string reference = "https://www.bilibili.com/avatar")
        {
            byte[] result;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = BiliSession.USER_AGENT;
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            Stream myResponseStream = response.GetResponseStream();
            BinaryReader streamReader = new BinaryReader(myResponseStream);
            result = streamReader.ReadBytes((int)response.ContentLength);
            streamReader.Close();
            myResponseStream.Close();
            return result;
        }
    }
}
