using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui
{
    public class SipUserAgent
    {
        public List<SipSession> Sessions { get; set; }
        public TcpClient Client { get; set; }
        public string SipServer { get; set; }
        public int SipPort { get; set; }

        public SipUserAgent(string sipServer, int sipPort)
        {
            Sessions = new List<SipSession>();
            SipServer = sipServer;
            SipPort = sipPort;
            Client = new TcpClient();
        }

        public async Task SendMessage(SipMessage message)
        {
            if (!Client.Connected)
            {
                await Client.ConnectAsync(SipServer, SipPort);
            }

            NetworkStream stream = Client.GetStream();

            var rawMessage = message.ConstructMessage();

            byte[] data = Encoding.ASCII.GetBytes(rawMessage);

            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task ReceiveMessage()
        {
            if (!Client.Connected)
            {
                await Client.ConnectAsync(SipServer, SipPort);
            }

            NetworkStream stream = Client.GetStream();

            byte[] data = new byte[256];

            string rawMessage = string.Empty;

            int bytes;
            while((bytes = await stream.ReadAsync(data, 0, data.Length)) != 0)
            {
                rawMessage += Encoding.ASCII.GetString(data, 0, bytes);
            }

            var message = new SipMessage("", new Dictionary<string, string>(), "");

            message.ParseMessage(rawMessage);
        }

        public void HandleReceivedMessage(SipMessage message)
        {
            var session = Sessions.FirstOrDefault(s => s.InitialInvite.Headers["From"] == message.Headers["To"]);

            if (session == null)
            {
                if(message.Method == "INVITE")
                {
                    session = new SipSession(message);
                    Sessions.Add(session);
                }
                else
                {
                    return;
                }
            }

            session.CurrentMessage = message;

            switch(message.Method)
            {
                case "INVITE":
                    var headers = new Dictionary<string, string>
                    {
                        { "To", message.Headers["From"] },
                        { "From", message.Headers["To"] },
                        { "CSeq", "1 INVITE" },
                        { "Call-ID", message.Headers["Call-ID"] },
                        { "Contact", "<sip:alice@atlanta.com;transport=tcp>" },
                        { "Content-Type", "application/sdp" },
                    };

                    var body = "v=0\r\no=alice 53655765 2353687637 IN IP4 pc33.atlanta.com\r\n...";

                    var response = new SipMessage("SIP/2.0 200 OK", headers, body);

                    SendMessage(response);

                    break;
                case "ACK":
                    session.EstablishSession();
                    break;
                case "BYE":
                    session.TerminateSession();
                    break;
            }
        }
    }
}
