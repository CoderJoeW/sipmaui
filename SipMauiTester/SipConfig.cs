using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMauiTester
{
    public class SipConfig
    {
        public string SipServer { get; set; }
        public int SipPort { get; set; }
        public string UserSipAddress { get; set; }
        public string TransportProtocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
