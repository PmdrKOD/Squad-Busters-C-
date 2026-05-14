using System.Net.Sockets;
using ReversedOfClans.Core;
using ReversedOfClans.Cryptography;

namespace ReversedOfClans.Message.Transmit;

public sealed class KeepAliveServerMessage : PiranhaMessage
{
    public KeepAliveServerMessage(NetworkStream conn, PepperEncrypter crypto) : base(  conn)
    {
    }
    public override int GetMessageType()
    {
        return 20108;
    }

    protected override void Encode()
    {
    }
}
