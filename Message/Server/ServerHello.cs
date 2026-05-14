using System.Net.Sockets;
using System.Security.Cryptography;
using ReversedOfClans.Core;
using ReversedOfClans.Cryptography;

namespace ReversedOfClans.Message.Transmit;

public sealed class ServerHello : PiranhaMessage
{


    public ServerHello(NetworkStream conn) : base( conn)
    {
    }

    public override ushort version()
    {
        return 0;
    }
    protected override void Encode()
    {
        Stream.WriteBytes(RandomNumberGenerator.GetBytes(24));
    }
    public override int GetMessageType()
    {
        return 20100;
    }
}
