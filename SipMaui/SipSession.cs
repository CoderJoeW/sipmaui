using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui
{
    public class SipSession
    {
        public SipMessage InitialInvite { get; set; }
        public SipMessage CurrentMessage { get; set; }

        public SipSession(SipMessage initialInvite) 
        {
            InitialInvite = initialInvite;
        }

        public void EstablishSession() 
        {
            var headers = new Dictionary<string, string>()
            {
                { "To", InitialInvite.Headers["To"] },
                { "From", InitialInvite.Headers["From"] }
            };

            var invite = new SipMessage("INVITE", headers, "");

            CurrentMessage = invite;
        }

        public void TerminateSession()
        {
            var headers = new Dictionary<string, string>()
            {
                { "To", InitialInvite.Headers["To"] },
                { "From", InitialInvite.Headers["From"] }
            };

            var bye = new SipMessage("BYE", headers, "");

            CurrentMessage = bye;
        }
    }
}
