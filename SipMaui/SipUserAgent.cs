﻿using System;
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
        public TcpClient TcpConnection { get; set; }
        public UdpClient UdpConnection { get; set; }
        public string SipServer { get; set; }
        public int SipPort { get; set; }
        public string UserSipAddress { get; set; }
        public string TransportProtocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public CancellationTokenSource ListeningCts { get; private set; }

        private SipMessageHelper _sipMessageHelper;

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
                    TcpConnection = new TcpClient();
                    break;
                case "udp":
                    UdpConnection = new UdpClient();
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
                    await ReceiveMessage();
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

            switch (TransportProtocol)
            {
                case "tcp":
                    if (!TcpConnection.Connected)
                    {
                        await TcpConnection.ConnectAsync(SipServer, SipPort);
                    }

                    NetworkStream stream = TcpConnection.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                    break;
                case "udp":
                    if (!UdpConnection.Client.Connected)
                    {
                        UdpConnection.Connect(SipServer, SipPort);
                    }
                    await UdpConnection.SendAsync(data, data.Length);
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
                    if (!TcpConnection.Connected)
                    {
                        await TcpConnection.ConnectAsync(SipServer, SipPort);
                    }

                    NetworkStream stream = TcpConnection.GetStream();

                    int bytes;
                    while ((bytes = await stream.ReadAsync(data, 0, data.Length)) != 0)
                    {
                        rawMessage += Encoding.ASCII.GetString(data, 0, bytes);
                    }
                    break;
                case "udp":
                    if (!UdpConnection.Client.Connected)
                    {
                        UdpConnection.Connect(SipServer, SipPort);
                    }

                    UdpReceiveResult result = await UdpConnection.ReceiveAsync();
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
            switch(message.Method)
            {
                case "401 Unauthorized":
                case "407 Proxy Authentication Required":
                    HandleAuthenticationChallenge(message);
                    break;
                case "OPTIONS":
                    _sipMessageHelper.RespondOptions(SipServer, SipPort, Username, TransportProtocol);
                    break;
                case "NOTIFY":
                    _sipMessageHelper.RespondNotify(SipServer, SipPort, Username, TransportProtocol);
                    break;
            }
        }

        public async Task HandleAuthenticationChallenge(SipMessage message)
        {
            Console.WriteLine(Password);
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

            var ha1 = ComputeMd5Hash($"{Username}:{realm}:{Password}");
            var ha2 = ComputeMd5Hash($"REGISTER:{UserSipAddress}");

            var response = (qop == "auth")
                ? ComputeMd5Hash($"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}")
                : ComputeMd5Hash($"{ha1}:{nonce}:{ha2}");



            await _sipMessageHelper.AuthenticateRegister(message, SipServer, SipPort, Username, UserSipAddress, TransportProtocol, realm, nonce, opaque, qop, nc, cnonce, algorithm, response);
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
