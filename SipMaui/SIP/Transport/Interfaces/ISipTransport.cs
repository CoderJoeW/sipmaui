using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui.SIP.Transport.Interfaces
{
    public interface ISipTransport
    {
        Task SendMessage(SipMessage message);
        Task<SipMessage> ReceiveMessage();
    }
}
