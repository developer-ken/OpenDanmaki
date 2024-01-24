using BiliveDanmakuAgent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki.Model
{
    public class DanmakuEventArgs : EventArgs
    {
        public Danmaku DanmakuObj { get; set; }
        public List<string> BandageImgUrls { get; set; }

        public DanmakuEventArgs(Danmaku dmk)
        {
            DanmakuObj = dmk;
        }
    }
}
