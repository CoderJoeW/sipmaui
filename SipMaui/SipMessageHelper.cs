using SipMaui.SIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui
{
    public class SipMessageHelper
    {
        private const string EXPIRE = "3600";
        private const string MAX_FORWARDS = "70";

        private Random _random = new Random();
        private SipUserAgent _userAgent;

        public SipMessageHelper(SipUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        public async Task Register(string sipServer, int sipPort, string username, string transport)
        {
            SipMessage sipMessage = new SipMessageBuilder()
                .WithMethod($"REGISTER sip:{username}@{sipServer}")
                .WithCommonHeaders(sipServer, sipPort, username, transport)
                .WithHeader("CSeq", "1 REGISTER")
                .Build();

            await _userAgent.SendMessage(sipMessage);
        }

        public async Task AuthenticateRegister(SipMessage message, string sipServer, int sipPort, string username, string userSipAddress, string transport, string realm, string nonce, string opaque, string qop, string nc, string cnonce, string algorithm, string response, string callId)
        {
            SipMessage sipMessage = new SipMessageBuilder()
                .WithMethod($"REGISTER {userSipAddress}")
                .WithCommonHeaders(sipServer, sipPort, username, transport, callId)
                .WithHeader("CSeq", "2 REGISTER")
                .WithHeader("Expires", EXPIRE)
                .WithHeader("Max-Forwards", MAX_FORWARDS)
                .WithHeader("Content-Length", "0")
                .WithHeader(message.Method == "401 Unauthorized" ? "Authorization" : "Proxy-Authorization", $"Digest username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{userSipAddress}\", response=\"{response}\", algorithm={algorithm}, opaque=\"{opaque}\", qop={qop}, nc={nc}, cnonce=\"{cnonce}\"")
                .Build();
            
            await _userAgent.SendMessage(sipMessage);
        }

        public async Task RespondWithOk(SipMessage message, string sipServer, int sipPort, string username, string transport, bool allowMethods = false)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Via", message.Headers["Via"] },
                { "From", message.Headers["From"] },
                { "To", message.Headers["To"] },
                { "Call-ID", message.Headers["Call-ID"] },
                { "CSeq", message.Headers["CSeq"] },
                { "Contact", message.Headers["Contact"] },
                { "Content-Length", "0" }
            };

            SipMessageBuilder builder = new SipMessageBuilder()
                .WithMethod("SIP/2.0 200 OK")
                .WithHeaders(headers);

            if (allowMethods)
            {
                builder.WithHeader("Allow", "INVITE, ACK, BYE, CANCEL, OPTIONS, MESSAGE, UPDATE, INFO, REGISTER");
            }
            
            await _userAgent.SendMessage(builder.Build());
        }
    }
}
