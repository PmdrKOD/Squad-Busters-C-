using System.Net.Sockets;
using ReversedOfClans.Core;
using ReversedOfClans.Cryptography;
using ReversedOfClans.Message.Receive;

namespace ReversedOfClans.Gate;

public static class MessageFactory
{
    public static async Task DispatchAsync(
        ushort id, 
        byte[] payload, 
        NetworkStream stream, 
        PepperEncrypter crypto, 
        CancellationToken cancellationToken = default
    )
    {
        switch (id)
        {
            case 10100:
            {
                new ClientHelloMessage(
                    payload, 
                    stream, 
                    crypto
                );
                break;
            }
            case 10101:
            {
                new Login(
                    payload, 
                    stream, 
                    crypto
                );
                break;
            }
      
            default:
                Logger.PacketNot(id);
                break;
        }
    }
}
