using SipMaui.SIP.Transport.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SipMaui.SIP.Transport
{
    public class SipUdpTransport : ISipTransport
    {
        private UdpClient _udpConnection;
        private string _sipServer;
        private int _sipPort;

        public SipUdpTransport(string sipServer, int sipPort)
        {
            _sipServer = sipServer;
            _sipPort = sipPort;
            _udpConnection = new UdpClient();
        }

        public async Task SendMessage(SipMessage message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message.ConstructMessage());

            if (!_udpConnection.Client.Connected)
            {
                _udpConnection.Connect(_sipServer, _sipPort);
            }
            await _udpConnection.SendAsync(data, data.Length);
        }

        public async Task<SipMessage> ReceiveMessage()
        {
            byte[] data = new byte[256];
            string rawMessage = string.Empty;

            if (!_udpConnection.Client.Connected)
            {
                _udpConnection.Connect(_sipServer, _sipPort);
            }

            UdpReceiveResult result = await _udpConnection.ReceiveAsync();
            rawMessage = Encoding.ASCII.GetString(result.Buffer);

            var message = new SipMessage("", new Dictionary<string, string>(), "");
            message.ParseMessage(rawMessage);

            return message;
        }
    }
}
