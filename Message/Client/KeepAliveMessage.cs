using System.Net.Sockets;
using ReversedOfClans.Cryptography;
using ReversedOfClans.Message.Transmit;

namespace ReversedOfClans.Message.Receive;

public sealed class KeepAliveMessage
{
    private readonly NetworkStream _conn;
    private readonly PepperEncrypter _crypto;

    public KeepAliveMessage(byte[] payload, NetworkStream conn, PepperEncrypter crypto)
    {
        _ = payload;
        _conn = conn;
        _crypto = crypto;
    }

    public Task Decode() => Task.CompletedTask;

    public Task Process(CancellationToken cancellationToken = default)
    {
        var msg = new KeepAliveServerMessage(_conn, _crypto);
        return msg.SendAsync(cancellationToken);
    }
}
