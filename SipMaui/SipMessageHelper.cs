using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui
{
    public class SipMessageHelper
    {
        private Random _random = new Random();

        private SipUserAgent _userAgent;

        public SipMessageHelper(SipUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        private Dictionary<string,string> CreateCommonHeaders(string sipServer, int sipPort, string userSipAddress, string transport, string callId = null)
        {
            return new Dictionary<string, string>
            {
                { "Via", $"SIP/2.0/UDP {sipServer}:{sipPort};branch=z9hG4bK{_random.Next()}" },
                { "To", $"<{userSipAddress}>" },
                { "From", $"<{userSipAddress}>" },
                { "Call-ID", callId ?? Guid.NewGuid().ToString() },
                { "Contact", $"<{userSipAddress};transport={transport}>" },
            };
        }

        public async Task Register(string sipServer, int sipPort, string username, string transport)
        {
            var headers = CreateCommonHeaders(sipServer, sipPort, username, transport);
            headers["CSeq"] = "1 REGISTER";

            var registerMessage = new SipMessage($"REGISTER {username}@{sipServer}", headers, "");
            await _userAgent.SendMessage(registerMessage);
        }

        public async Task AuthenticateRegister(SipMessage message, string sipServer, int sipPort, string username, string userSipAddress, string transport, string realm, string nonce, string opaque, string qop, string nc, string cnonce, string algorithm, string response)
        {
            var headers = CreateCommonHeaders(sipServer, sipPort, userSipAddress, transport, message.Headers["Call-ID"]);
            headers["CSeq"] = "2 REGISTER";
            headers["Expires"] = "3600";
            headers["Max-Forwards"] = "70";
            headers["Content-Length"] = "0";
            headers[message.Method == "401 Unauthorized" ? "Authorization" : "Proxy-Authorization"] = $"Digest username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{userSipAddress}\", response=\"{response}\", algorithm={algorithm}, opaque=\"{opaque}\", qop={qop}, nc={nc}, cnonce=\"{cnonce}\"";

            var authenticateRegisterMessage = new SipMessage($"REGISTER {userSipAddress}", headers, message.Body);
            await _userAgent.SendMessage(authenticateRegisterMessage);
        }
    }
}
