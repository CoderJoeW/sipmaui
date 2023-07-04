# SIP-Maui

SIP-Maui is a .NET based library for handling SIP (Session Initiation Protocol) operations, such as registering a SIP user, initiating and terminating calls, handling SIP messages, and managing SIP sessions. The library can be used to create Voice over IP (VoIP) applications using SIP.

## Features

- Easy handling of SIP messages with SipMessage and SipMessageBuilder classes.
- Complete SIP session management with the SipSession class.
- Ability to listen for incoming SIP messages and handle different types of SIP requests (INVITE, BYE, OPTIONS etc.)
- Support for both TCP and UDP transport protocols.
- Authentication handling for SIP registrations.

## Getting Started

```csharp
// Create a new SIP user agent
SipUserAgent userAgent = new SipUserAgent("sipserver.com", 5060, "sip:user@sipserver.com", "udp", "username", "password");

// Start listening for SIP messages
userAgent.StartListeningForSipMessages();

// Initiate a call
userAgent.InitiateCall("sip:otheruser@sipserver.com");
```

## Dependencies

-   .NET 6.0 or higher

## Build

1.  Clone the repository
2.  Build the solution using any .NET compatible IDE (like Visual Studio or JetBrains Rider)

## Contribute

Contributions are welcome. Please open an issue or submit a pull request.

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).