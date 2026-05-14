using System.Globalization;
using System.Net.Sockets;
using ReversedOfClans.Core;
using ReversedOfClans.Cryptography;

namespace ReversedOfClans.Message.Transmit;

public sealed class LoginOkMessage : PiranhaMessage
{
    public LoginOkMessage(NetworkStream conn) : base( conn)
    {
    }

    protected override void Encode()
    {
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(2);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(2);
        Stream.WriteString("uxkNE0L1y7gx2J695K1DSJzqiIKIK9UDmTAgbKu4");
        Stream.WriteString(null);
        Stream.WriteString(null);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(1);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteBoolean(true);
        Stream.WriteBoolean(false);
        Stream.WriteBoolean(false);
        Stream.WriteBoolean(false);
        Stream.WriteBoolean(true);
        Stream.WriteBoolean(false);
        Stream.WriteBoolean(false);
        Stream.WriteBoolean(true);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(2);
        Stream.WriteVInt(4);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(11);
        Stream.WriteString("prod");
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteString(null);
        Stream.WriteString("1778744053.3048027");
        Stream.WriteString("1714237625000");
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteString(null);
        Stream.WriteString("GR");
        Stream.WriteString(null);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(3);
        Stream.WriteString("https://event-assets.squadbustersgame.com");
        Stream.WriteString("https://game-assets.squadbustersgame.com");
        Stream.WriteString(null);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(12);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteBoolean(true);
        Stream.WriteBoolean(false);
        Stream.WriteBoolean(false);
        Stream.WriteBoolean(true);
        Stream.WriteBoolean(true);
        Stream.WriteBoolean(true);
        Stream.WriteBoolean(true);
        Stream.WriteBoolean(false);
        Stream.WriteVInt(220);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(1);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
        Stream.WriteVInt(0);
    }
     public override int GetMessageType()
    {
        return 29125;
    }
}
