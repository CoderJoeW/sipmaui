using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui
{
    public class SipUserAgent
    {
        public List<SipSession> Sessions { get; set; }

        public SipUserAgent()
        {
            Sessions = new List<SipSession>();
        }

        public void SendMessage(SipMessage message) { }
        public void ReceiveMessage(string message) { }
    }
}
