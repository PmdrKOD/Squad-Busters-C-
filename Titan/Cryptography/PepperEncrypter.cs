using System.Security.Cryptography;

namespace ReversedOfClans.Cryptography;

public enum PepperState
{
    Invalid = -1,
    Auth = 0,
    Login = 1,
    Authenticated = 2
}

public sealed class PepperEncrypter
{
    private static readonly byte[] ServerPublicKey = Convert.FromHexString("439CF001F04AACD0E47A941C62FA73FC450769BD348BC71A9FEE3806D84C4D16");
    private static readonly byte[] ClientPrivateKey =
    [
        0xFF, 0x45, 0x12, 0x7A, 0x9C, 0x23, 0x4B, 0x67,
        0xA1, 0x2D, 0x3E, 0x56, 0x90, 0xAB, 0xC8, 0xD3,
        0xE5, 0xF4, 0x6B, 0x72, 0x85, 0x19, 0x3A, 0x4F,
        0x28, 0x63, 0x92, 0xBD, 0xFA, 0x34, 0x76, 0x08
    ];

    private readonly byte[] _clientPublicKey;
    private readonly Nonce _encryptNonce;
    private readonly byte[] _sharedEncryptionKey;
    private byte[]? _sessionKey;
    private Nonce? _decryptNonce;
    private byte[]? _sharedLoginKey;

    public PepperEncrypter()
    {
        State = PepperState.Invalid;
        _clientPublicKey = TweetNaCl.ScalarMultBase(ClientPrivateKey);
        _encryptNonce = Nonce.Random();
        _sharedEncryptionKey = RandomNumberGenerator.GetBytes(32);
    }

    public PepperState State { get; private set; }

    public byte[] Decrypt(ushort packetId, byte[] payload)
    {
        switch (packetId)
        {
            case 10100:
                if (State != PepperState.Invalid)
                {
                    throw new CryptographicException("Received ClientHelloMessage while not in Invalid state.");
                }

                State = PepperState.Auth;
                return payload;

            case 10101:
                if (State != PepperState.Auth)
                {
                    throw new CryptographicException("Received LoginMessage while not in Auth state.");
                }

                if (payload.Length < 32 || !payload.AsSpan(0, 32).SequenceEqual(_clientPublicKey))
                {
                    throw new CryptographicException("LoginMessage client public key does not match.");
                }

                if (_sessionKey is null)
                {
                    throw new CryptographicException("Server session key is missing.");
                }

                byte[] cipher = payload[32..];
                Nonce loginNonce = Nonce.FromKeys(_clientPublicKey, ServerPublicKey);
                _sharedLoginKey = TweetNaCl.BoxBeforeNm(ServerPublicKey, ClientPrivateKey);
                byte[] decrypted = TweetNaCl.SecretBoxOpen(cipher, loginNonce.Bytes, _sharedLoginKey);

                if (decrypted.Length < 48 || !decrypted.AsSpan(0, 24).SequenceEqual(_sessionKey))
                {
                    throw new CryptographicException("LoginMessage session key does not match.");
                }

                _decryptNonce = new Nonce(decrypted[24..48]);
                State = PepperState.Login;
                return decrypted[48..];

            default:
                if (State != PepperState.Authenticated)
                {
                    throw new CryptographicException("Session is not authenticated.");
                }

                if (_decryptNonce is null)
                {
                    throw new CryptographicException("Decrypt nonce is missing.");
                }

                _decryptNonce.Increment();
                return TweetNaCl.SecretBoxOpen(payload, _decryptNonce.Bytes, _sharedEncryptionKey);
        }
    }

    public byte[] Encrypt(ushort packetId, byte[] payload)
    {
        switch (State)
        {
            case PepperState.Auth:
                if (packetId == 20100)
                {
                    if (payload.Length < 28)
                    {
                        throw new CryptographicException("ServerHelloMessage payload is too short.");
                    }

                    _sessionKey = payload[4..];
                    return payload;
                }

                if (packetId == 20103)
                {
                    return payload;
                }

                throw new CryptographicException("Only 20100 and 20103 can be sent in Auth state.");

            case PepperState.Login:
                if (_decryptNonce is null || _sharedLoginKey is null)
                {
                    throw new CryptographicException("Login crypto state is incomplete.");
                }

                Nonce nonce = Nonce.FromKeys(_decryptNonce.Bytes, _clientPublicKey, ServerPublicKey);
                byte[] loginPayload = new byte[24 + 32 + payload.Length];
                Buffer.BlockCopy(_encryptNonce.Bytes, 0, loginPayload, 0, 24);
                Buffer.BlockCopy(_sharedEncryptionKey, 0, loginPayload, 24, 32);
                Buffer.BlockCopy(payload, 0, loginPayload, 56, payload.Length);
                State = PepperState.Authenticated;
                return TweetNaCl.SecretBox(loginPayload, nonce.Bytes, _sharedLoginKey);

            case PepperState.Authenticated:
                _encryptNonce.Increment();
                return TweetNaCl.SecretBox(payload, _encryptNonce.Bytes, _sharedEncryptionKey);

            default:
                throw new CryptographicException("Session is not ready for encryption.");
        }
    }

    private sealed class Nonce
    {
        public Nonce(byte[] bytes)
        {
            if (bytes.Length != 24)
            {
                throw new ArgumentException("Nonce must be 24 bytes.", nameof(bytes));
            }

            Bytes = bytes.ToArray();
        }

        public byte[] Bytes { get; private set; }

        public static Nonce Random() => new(RandomNumberGenerator.GetBytes(24));

        public static Nonce FromKeys(params byte[][] values)
        {
            int length = values.Sum(value => value.Length);
            byte[] data = new byte[length];
            int offset = 0;
            foreach (byte[] value in values)
            {
                Buffer.BlockCopy(value, 0, data, offset, value.Length);
                offset += value.Length;
            }

            return new Nonce(Blake2b.Hash(data, 24));
        }

        public void Increment()
        {
            int carry = 2;
            for (int i = 0; i < Bytes.Length && carry != 0; i++)
            {
                int value = Bytes[i] + carry;
                Bytes[i] = (byte)value;
                carry = value >> 8;
            }
        }
    }
}
