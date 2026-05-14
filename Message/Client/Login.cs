using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using ReversedOfClans.Core;
using ReversedOfClans.Cryptography;
using ReversedOfClans.Message.Transmit;

namespace ReversedOfClans.Message.Receive;

public sealed class Login
{
    private readonly ByteStream _stream;
    private readonly NetworkStream _conn;
    private readonly PepperEncrypter _crypto;

    public int HighId { get; private set; }
    public int LowId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int Build { get; private set; }

    public Login(
        byte[] payload,
        NetworkStream conn,
        PepperEncrypter crypto,
        CancellationToken cancellationToken = default
    )
    {
        _stream = new ByteStream(payload);
        _conn = conn;
        _crypto = crypto;
        Off_loginOk(cancellationToken);

    }
    public void Off_loginOk(CancellationToken cancellationToken = default)
    {
        Decode();
        Process(cancellationToken);
    }

    public void  Decode()
    {
        HighId = _stream.ReadInt();
        LowId = _stream.ReadInt();
        Token = _stream.ReadString();
        Major = _stream.ReadInt();
        Minor = _stream.ReadInt();
        Build = _stream.ReadInt();
        _stream.ReadString();
        _stream.ReadString();
        _stream.ReadString();
        _stream.ReadString();
        _stream.ReadString();
        _stream.ReadLong();
        _stream.ReadString();
        _stream.ReadString();
        _stream.ReadString();
        _stream.ReadBoolean();
        
    }

    public async Task Process(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        var ok = new LoginOkMessage(_conn);
         ok._crypto = _crypto;
        await ok.SendAsync(cancellationToken);

        var home = new OwnHomeDataMessage(_conn);
                 home._crypto = _crypto;
        await home.SendAsync(cancellationToken);
    }
}
