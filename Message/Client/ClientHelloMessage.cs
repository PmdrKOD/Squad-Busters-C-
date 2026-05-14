using System.Net.Sockets;
using ReversedOfClans.Cryptography;
using ReversedOfClans.Message.Transmit;

namespace ReversedOfClans.Message.Receive;

public sealed class ClientHelloMessage
{
    private readonly NetworkStream _conn;
    public  PepperEncrypter _crypto;

    public ClientHelloMessage(byte[] payload, NetworkStream conn, PepperEncrypter crypto)
    {
        _ = payload;
        _conn = conn;
        _crypto = crypto;
        Off_HELLO();
    }

    public Task Decode() => Task.CompletedTask;
    public void Off_HELLO(CancellationToken cancellationToken = default)
    {
        Decode();
        Process(cancellationToken);
    }
    public Task Process(CancellationToken cancellationToken = default)
    {
        var msg = new ServerHello(_conn);
        msg._crypto = _crypto;
        return msg.SendAsync(cancellationToken);
    }
}
