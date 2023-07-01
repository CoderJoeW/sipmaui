using SipMaui.SIP.Transport.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui.SIP.Transport
{
    public class SipTcpTransport: ISipTransport
    {
        private TcpClient _tcpConnection;
        private string _sipServer;
        private int _sipPort;
        private NetworkStream _stream;

        public SipTcpTransport(string sipServer, int sipPort)
        {
            _sipServer = sipServer;
            _sipPort = sipPort;
            _tcpConnection = new TcpClient();
        }

        public async Task SendMessage(SipMessage message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message.ConstructMessage());

            if (!_tcpConnection.Connected)
            {
                await _tcpConnection.ConnectAsync(_sipServer, _sipPort);
            }

            _stream = _tcpConnection.GetStream();
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public async Task<SipMessage> ReceiveMessage()
        {
            byte[] data = new byte[256];
            string rawMessage = string.Empty;

            if (!_tcpConnection.Connected)
            {
                await _tcpConnection.ConnectAsync(_sipServer, _sipPort);
            }

            _stream = _tcpConnection.GetStream();

            int bytes;
            while ((bytes = await _stream.ReadAsync(data, 0, data.Length)) != 0)
            {
                rawMessage += Encoding.ASCII.GetString(data, 0, bytes);
            }

            var message = new SipMessage("", new Dictionary<string, string>(), "");
            message.ParseMessage(rawMessage);

            return message;
        }
    }
}
