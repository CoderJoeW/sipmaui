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
    }
}
