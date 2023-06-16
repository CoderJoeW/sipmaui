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

        public void EstablishSession() { }
        public void TerminateSession() { }
    }
}
