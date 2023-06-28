using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui
{
    public class SipMessageHelper
    {
        public static async Task Register(SipUserAgent agent, string sipServer, int sipPort, string username, string transport)
        {
            string userSipAddress = $"sip:{username}@{sipServer}";
            var headers = new Dictionary<string, string>
            {
                { "Via", $"SIP/2.0/UDP {sipServer}:{sipPort};branch=z9hG4bK{new Random().Next()}" },
                { "To", $"<{userSipAddress}>" },
                { "From", $"<{userSipAddress}>" },
                { "CSeq", "1 REGISTER" },
                { "Call-ID", Guid.NewGuid().ToString() },
                { "Contact", $"<sip:{username}@{sipServer};transport={transport}>" },
            };

            var registerMessage = new SipMessage($"REGISTER {userSipAddress}", headers, "");
            await agent.SendMessage(registerMessage);
        }

        public static async Task AuthenticateRegister(SipUserAgent agent, SipMessage message, string sipServer, int sipPort, string username, string userSipAddress, string transport, string realm, string nonce, string opaque, string qop, string nc, string cnonce, string algorithm, string response)
        {
            var headers = new Dictionary<string, string>
            {
                { "Via", $"SIP/2.0/UDP {sipServer}:{sipPort};branch=z9hG4bK{new Random().Next()}" },
                { "To", $"<{userSipAddress}>" },
                { "From", $"<{userSipAddress}>" },
                { "CSeq", "2 REGISTER" },
                { "Call-ID", message.Headers["Call-ID"] },
                { "Contact", $"<{userSipAddress};transport={transport}>" },
                { "Expires", "3600" },
                { "Max-Forwards", "70" },
                { "Content-Length", "0" },
                { message.Method == "401 Unauthorized" ? "Authorization" : "Proxy-Authorization", $"Digest username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{userSipAddress}\", response=\"{response}\", algorithm={algorithm}, opaque=\"{opaque}\", qop={qop}, nc={nc}, cnonce=\"{cnonce}\"" },
            };

            var newMessage = new SipMessage($"REGISTER {userSipAddress}", headers, message.Body);
            await agent.SendMessage(newMessage);
        }

    }
}
