using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace OpenDanmaki.Model
{
    public class HttpRequestEventArgs : EventArgs
    {
        public bool IsHandled
        {
            get => handled;
            set
            {
                if (handled && !value) throw new InvalidOperationException("Property IsHandled is one-way changeable.");
                handled = value;
            }
        }

        public HttpContextEventArgs Request
        {
            get
            {
                if (handled) throw new InvalidOperationException("This event has been handled!");
                return request;
            }
        }

        private HttpContextEventArgs request;
        private bool handled = false;
        public HttpRequestEventArgs(HttpContextEventArgs req)
        {
            request = req;
        }
    }
}
