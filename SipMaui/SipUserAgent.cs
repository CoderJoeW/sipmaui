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

        public void SendMessage(SipMessage message)
        {
            var rawMessage = message.ConstructMessage();
        }

        public void ReceiveMessage(string rawMessage)
        {
            var message = new SipMessage("", new Dictionary<string, string>(), "");

            message.ParseMessage(rawMessage);
        }
    }
}
