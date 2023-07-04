using SipMaui.SIP.Enums;
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
        public SessionState State { get; private set; }

        public SipSession(SipMessage initialInvite) 
        {
            InitialInvite = initialInvite;
            State = SessionState.Idle;
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

            State = SessionState.Ringing;
        }

        public void AcceptCall()
        {
            if(State == SessionState.Ringing )
            {
                State = SessionState.InProgress;
            }
            else
            {
                throw new InvalidOperationException("Call can only be accepted in the 'Ringing' state.");
            }
        }

        public void HoldCall()
        {
            if(State == SessionState.Ringing)
            {
                State = SessionState.Hold;
            }
            else
            {
                throw new InvalidOperationException("Call can only be held in the 'InProgress' state.");
            }
        }

        public void ResumeCall()
        {
            if(State == SessionState.Hold)
            {
                State = SessionState.InProgress;
            }
            else
            {
                throw new InvalidOperationException("Call can only be resumed in the 'Hold' state.");
            }
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

            State = SessionState.Terminated;
        }
    }
}
