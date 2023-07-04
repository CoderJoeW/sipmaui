using Microsoft.Win32;
using Newtonsoft.Json;
using SipMaui;
using SipMauiTester;

string json = File.ReadAllText("config.json");

SipConfig config = JsonConvert.DeserializeObject<SipConfig>(json);

var userAgent = new SipUserAgent(config.SipServer, config.SipPort, config.UserSipAddress, config.TransportProtocol, config.Username, config.Password);
SipMessageHelper sipMessageHelper = new SipMessageHelper(userAgent);

userAgent.MessageReceived += (message) =>
{
    if (message.Method == "OPTIONS") return;
    if (message.Method == "NOTIFY") return;
    Console.WriteLine("Received message: " + message.Method);
};

userAgent.MessageSent += (message) =>
{
    Console.WriteLine("Sent message: " + message.Method);
};

userAgent.StartListeningForSipMessages();

await sipMessageHelper.Register(config.SipServer, config.SipPort, config.Username, config.TransportProtocol);

Console.WriteLine("Press any key to stop listening...");
Console.ReadKey();

await userAgent.InitiateCall("7432239193@128.136.225.152");

Console.WriteLine("Press any key to stop listening...");
Console.ReadKey();

userAgent.StopListeningForSipMessages();