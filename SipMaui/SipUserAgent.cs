using SipMaui.SIP.Transport;
using SipMaui.SIP.Transport.Interfaces;
using SipMaui.Utils;
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
        public event Action<SipMessage> MessageReceived;
        public event Action<SipMessage> MessageSent;

        public List<SipSession> Sessions { get; set; }

        public string SipServer { get; set; }
        public int SipPort { get; set; }
        public string UserSipAddress { get; set; }
        public string TransportProtocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public CancellationTokenSource ListeningCts { get; private set; }

        private SipMessageHelper _sipMessageHelper;
        private ISipTransport _sipTransport;

        public SipUserAgent(string sipServer, int sipPort, string userSipAddress, string transportProtocol, string username, string password)
        {
            Sessions = new List<SipSession>();
            SipServer = sipServer;
            SipPort = sipPort;
            UserSipAddress = userSipAddress;
            TransportProtocol = transportProtocol;
            Username = username;
            Password = password;
            switch (TransportProtocol)
            {
                case "tcp":
                    _sipTransport = new SipTcpTransport(SipServer, SipPort);
                    break;
                case "udp":
                    _sipTransport = new SipUdpTransport(SipServer, SipPort);
                    break;
                default:
                    throw new ArgumentException("Unsupported transport protocol. Use either \"tcp\" or \"udp\".");
            }
            _sipMessageHelper = new SipMessageHelper(this);
        }

        public void StartListeningForSipMessages()
        {
            ListeningCts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!ListeningCts.IsCancellationRequested)
                {
                    SipMessage sipMessage = await _sipTransport.ReceiveMessage();

                    MessageReceived?.Invoke(sipMessage);

                    HandleReceivedMessage(sipMessage);
                }
            });
        }

        public void StopListeningForSipMessages()
        {
            ListeningCts?.Cancel();
        }

        public async Task SendMessage(SipMessage message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message.ConstructMessage());

            await _sipTransport.SendMessage(message);

            MessageSent?.Invoke(message);
        }

        public void HandleReceivedMessage(SipMessage message)
        {
            switch(message.Method)
            {
                case "401 Unauthorized":
                case "407 Proxy Authentication Required":
                    HandleAuthenticationChallenge(message);
                    break;
                case "OPTIONS":
                    _sipMessageHelper.RespondWithOk(SipServer, SipPort, Username, TransportProtocol, true);
                    break;
                case "NOTIFY":
                    _sipMessageHelper.RespondWithOk(SipServer, SipPort, Username, TransportProtocol);
                    break;
            }
        }

        public async Task HandleAuthenticationChallenge(SipMessage message)
        {
            var authenticateHeader = message.Method == "401 Unauthorized" ? "WWW-Authenticate" : "Proxy-Authenticate";
            var authenticateValue = message.Headers[authenticateHeader];
            var parameters = authenticateValue.Split(',').Select(param => param.Trim().Split('=')).ToDictionary(parts => parts[0], parts => parts[1].Trim('"'));

            var nonce = parameters["nonce"];
            var realm = parameters["Digest realm"];
            var opaque = parameters.ContainsKey("opaque") ? parameters["opaque"] : null;
            var qop = parameters.ContainsKey("qop") ? parameters["qop"] : null;
            var algorithm = parameters.ContainsKey("algorithm") ? parameters["algorithm"] : "MD5";

            var nc = "00000001";
            var cnonce = new Random().Next(123400, 9999999).ToString("x");

            var ha1 = Hashing.Md5($"{Username}:{realm}:{Password}");
            var ha2 = Hashing.Md5($"REGISTER:{UserSipAddress}");

            var response = (qop == "auth")
                ? Hashing.Md5($"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}")
                : Hashing.Md5($"{ha1}:{nonce}:{ha2}");



            await _sipMessageHelper.AuthenticateRegister(message, SipServer, SipPort, Username, UserSipAddress, TransportProtocol, realm, nonce, opaque, qop, nc, cnonce, algorithm, response);
        }
    }
}
