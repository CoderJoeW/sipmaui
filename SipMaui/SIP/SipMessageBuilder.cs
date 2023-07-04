using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipMaui.SIP
{
    public class SipMessageBuilder
    {
        private string _method;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        private string _body;

        public SipMessageBuilder WithMethod(string method)
        {
            _method = method;

            return this;
        }

        public SipMessageBuilder WithHeader(string key, string value)
        {
            _headers.Add(key, value);

            return this;
        }

        public SipMessageBuilder WithHeaders(Dictionary<string, string> headers)
        {
            _headers = headers;

            return this;
        }

        public SipMessageBuilder WithBody(string body)
        {
            _body = body;

            return this;
        }

        public SipMessageBuilder WithCommonHeaders(string sipServer, int sipPort, string username, string transport, string callId = null)
        {
            _headers["Via"] = $"SIP/2.0/UDP {sipServer}:{sipPort};branch=z9hG4bK{new Random().Next()}";
            _headers["To"] = $"<sip:{username}@{sipServer}>";
            _headers["From"] = $"<sip:{username}@{sipServer}>";
            _headers["Call-ID"] = callId ?? Guid.NewGuid().ToString();
            _headers["Contact"] = $"<sip:{username}@{sipServer};transport={transport}>";

            return this;
        }

        public SipMessage Build()
        {
            return new SipMessage(_method, _headers, _body);
        }
    }
}
