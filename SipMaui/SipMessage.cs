using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui
{
    public class SipMessage
    {
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        public SipMessage(string method, Dictionary<string, string> headers, string body) 
        { 
            Method = method;
            Headers = headers;
            Body = body;
        }

        public void ParseMessage(string message)
        {
            var lines = message.Split("\r\n");

            var startLineParts = lines[0].Split(' ');

            if (startLineParts[0] == "SIP/2.0")
            {
                Method = string.Join(" ", startLineParts.Skip(1));
            }
            else
            {
                Method = startLineParts[0];
            }

            Headers = new Dictionary<string, string>();
            var headerLines = lines.Skip(1).TakeWhile(line => line != "").ToList();

            foreach (var line in headerLines)
            {
                var parts = line.Split(':');
                var name = parts[0].Trim();
                var value = parts[1].Trim();

                Headers[name] = value;
            }

            Body = string.Join("\r\n", lines.Skip(headerLines.Count + 2));
        }

        public string ConstructMessage() 
        {
            var headerLines = Headers.Select(header => $"{header.Key}: {header.Value}");
            var message = $"{Method} SIP/2.0\r\n{string.Join("\r\n", headerLines)}\r\n\r\n{Body}";

            return message;
        }
    }
}
