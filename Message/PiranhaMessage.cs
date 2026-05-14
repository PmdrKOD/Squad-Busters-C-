using System.Buffers.Binary;
using System.Net.Sockets;
using ReversedOfClans.Cryptography;

namespace ReversedOfClans.Core;

public abstract class PiranhaMessage
{
    protected readonly ByteStream Stream;


    public NetworkStream _conn;
    public PepperEncrypter _crypto;
    protected PiranhaMessage(NetworkStream conn)
    {
        Stream = new ByteStream();
        _conn = conn;
        _crypto = new PepperEncrypter();
    }
    
    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        if ((ushort)GetMessageType() < 20_000)
        {
            return;
        }

        Encode();
        byte[] body = _crypto.Encrypt((ushort)GetMessageType(), Stream.ToArray());
        byte[] header = new byte[7];

        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(0, 2), (ushort)GetMessageType());
        header[2] = (byte)((body.Length >> 16) & 0xFF);
        header[3] = (byte)((body.Length >> 8) & 0xFF);
        header[4] = (byte)(body.Length & 0xFF);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(5, 2), version());

        await _conn.WriteAsync(header, cancellationToken);
        await _conn.WriteAsync(body, cancellationToken);
        await _conn.FlushAsync(cancellationToken);

    }

    protected virtual void Encode(){}
    public virtual int GetMessageType()
    {
        return 0;
    }
    public virtual ushort version() //btw
    {
        return 999;
    }


}
