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
        public TcpClient TcpClient { get; set; }
        public UdpClient UdpClient { get; set; }
        public string SipServer { get; set; }
        public int SipPort { get; set; }
        public string UserSipAddress { get; set; }
        public string TransportProtocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public CancellationTokenSource ListeningCts { get; private set; }

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
                    TcpClient = new TcpClient();
                    break;
                case "udp":
                    UdpClient = new UdpClient();
                    break;
                default:
                    throw new ArgumentException("Unsupported transport protocol. Use either \"tcp\" or \"udp\".");
            }
        }

        public void StartListening()
        {
            ListeningCts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!ListeningCts.IsCancellationRequested)
                {
                    await ReceiveMessage();
                }
            });
        }

        public void StopListening()
        {
            ListeningCts?.Cancel();
        }

        public async Task SendMessage(SipMessage message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message.ConstructMessage());

            switch (TransportProtocol)
            {
                case "tcp":
                    if (!TcpClient.Connected)
                    {
                        await TcpClient.ConnectAsync(SipServer, SipPort);
                    }

                    NetworkStream stream = TcpClient.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                    break;
                case "udp":
                    if (!UdpClient.Client.Connected)
                    {
                        UdpClient.Connect(SipServer, SipPort);
                    }
                    await UdpClient.SendAsync(data, data.Length);
                    break;
            }

            MessageSent?.Invoke(message);
        }

        public async Task ReceiveMessage()
        {
            byte[] data = new byte[256];
            string rawMessage = string.Empty;

            switch (TransportProtocol)
            {
                case "tcp":
                    if (!TcpClient.Connected)
                    {
                        await TcpClient.ConnectAsync(SipServer, SipPort);
                    }

                    NetworkStream stream = TcpClient.GetStream();

                    int bytes;
                    while ((bytes = await stream.ReadAsync(data, 0, data.Length)) != 0)
                    {
                        rawMessage += Encoding.ASCII.GetString(data, 0, bytes);
                    }
                    break;
                case "udp":
                    if (!UdpClient.Client.Connected)
                    {
                        UdpClient.Connect(SipServer, SipPort);
                    }

                    UdpReceiveResult result = await UdpClient.ReceiveAsync();
                    rawMessage = Encoding.ASCII.GetString(result.Buffer);
                    break;
            }

            var message = new SipMessage("", new Dictionary<string, string>(), "");
            message.ParseMessage(rawMessage);

            MessageReceived?.Invoke(message);

            HandleReceivedMessage(message);
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
                        { "Contact", $"<sip:{UserSipAddress};transport={TransportProtocol}>" },
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
                case "401 Unauthorized":
                case "407 Proxy Authentication Required":
                    HandleAuthentication(message);
                    break;
            }
        }

        public void HandleAuthentication(SipMessage message)
        {
            var authenticateHeader = message.Method == "401 Unauthorized" ? "WWW-Authenticate" : "Proxy-Authenticate";
            var authenticateValue = message.Headers[authenticateHeader];
            var parameters = authenticateValue.Split(',').Select(param => param.Trim().Split('=')).ToDictionary(parts => parts[0], parts => parts[1].Trim('"'));

            var nonce = parameters["nonce"];
            var realm = parameters["realm"];

            var ha1 = ComputeMd5Hash($"{Username}:{realm}:{Password}");
            var ha2 = ComputeMd5Hash($"{message.Method}:{UserSipAddress}");
            var response = ComputeMd5Hash($"{ha1}:{nonce}:{ha2}");

            var headers = new Dictionary<string, string>
            {
                { "To", message.Headers["From"] },
                { "From", message.Headers["To"] },
                { "CSeq", "2 " + message.Method },
                { "Call-ID", message.Headers["Call-ID"] },
                { "Contact", $"<sip:{UserSipAddress};transport={TransportProtocol}>" },
                { message.Method == "401 Unauthorized" ? "Authorization" : "Proxy-Authorization", $"Digest username=\"{Username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{UserSipAddress}\", response=\"{response}\"" },
            };

            var newMessage = new SipMessage(message.Method, headers, message.Body);
            SendMessage(newMessage);
        }

        public string ComputeMd5Hash(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

    }
}
