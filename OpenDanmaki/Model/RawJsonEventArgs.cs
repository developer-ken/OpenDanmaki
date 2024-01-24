using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDanmaki.Model
{
    public class RawJsonEventArgs : EventArgs
    {
        public JObject RawJson;
        public bool Drop;
        public RawJsonEventArgs(JObject json)
        {
            this.RawJson = json;
        }
    }
}
