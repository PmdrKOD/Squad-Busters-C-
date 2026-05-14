using System.Numerics;
using System.Security.Cryptography;

namespace ReversedOfClans.Cryptography;

internal static class TweetNaCl
{
    private static readonly byte[] Sigma = "expand 32-byte k"u8.ToArray();
    private static readonly BigInteger P = (BigInteger.One << 255) - 19;

    public static byte[] SecretBox(byte[] message, byte[] nonce, byte[] key)
    {
        byte[] paddedMessage = new byte[message.Length + 32];
        Buffer.BlockCopy(message, 0, paddedMessage, 32, message.Length);

        byte[] paddedCipher = XorStream(paddedMessage, nonce, key);
        byte[] tag = Poly1305(paddedCipher.AsSpan(32).ToArray(), paddedCipher.AsSpan(0, 32).ToArray());
        Buffer.BlockCopy(tag, 0, paddedCipher, 16, 16);
        Array.Clear(paddedCipher, 0, 16);
        return paddedCipher[16..];
    }

    public static byte[] SecretBoxOpen(byte[] cipher, byte[] nonce, byte[] key)
    {
        byte[] paddedCipher = new byte[cipher.Length + 16];
        Buffer.BlockCopy(cipher, 0, paddedCipher, 16, cipher.Length);

        byte[] polyKey = Stream(32, nonce, key);
        byte[] expected = Poly1305(paddedCipher.AsSpan(32).ToArray(), polyKey);
        if (!CryptographicOperations.FixedTimeEquals(expected, paddedCipher.AsSpan(16, 16)))
        {
            throw new CryptographicException("crypto_secretbox_open failed.");
        }

        byte[] paddedMessage = XorStream(paddedCipher, nonce, key);
        Array.Clear(paddedMessage, 0, 32);
        return paddedMessage[32..];
    }

    public static byte[] BoxBeforeNm(byte[] publicKey, byte[] privateKey)
    {
        byte[] shared = ScalarMult(privateKey, publicKey);
        return HSalsa20(new byte[16], shared);
    }

    public static byte[] ScalarMultBase(byte[] privateKey)
    {
        byte[] basePoint = new byte[32];
        basePoint[0] = 9;
        return ScalarMult(privateKey, basePoint);
    }

    private static byte[] ScalarMult(byte[] scalar, byte[] point)
    {
        if (scalar.Length != 32 || point.Length != 32)
        {
            throw new ArgumentException("X25519 keys must be 32 bytes.");
        }

        byte[] k = scalar.ToArray();
        k[0] &= 248;
        k[31] &= 127;
        k[31] |= 64;

        byte[] uBytes = point.ToArray();
        uBytes[31] &= 127;
        BigInteger x1 = FromLittleEndian(uBytes);
        BigInteger x2 = BigInteger.One;
        BigInteger z2 = BigInteger.Zero;
        BigInteger x3 = x1;
        BigInteger z3 = BigInteger.One;
        int swap = 0;

        for (int t = 254; t >= 0; t--)
        {
            int kt = (k[t >> 3] >> (t & 7)) & 1;
            swap ^= kt;
            ConditionalSwap(ref x2, ref x3, swap);
            ConditionalSwap(ref z2, ref z3, swap);
            swap = kt;

            BigInteger a = Mod(x2 + z2);
            BigInteger aa = Mod(a * a);
            BigInteger b = Mod(x2 - z2);
            BigInteger bb = Mod(b * b);
            BigInteger e = Mod(aa - bb);
            BigInteger c = Mod(x3 + z3);
            BigInteger d = Mod(x3 - z3);
            BigInteger da = Mod(d * a);
            BigInteger cb = Mod(c * b);
            x3 = Mod((da + cb) * (da + cb));
            z3 = Mod(x1 * Mod((da - cb) * (da - cb)));
            x2 = Mod(aa * bb);
            z2 = Mod(e * Mod(aa + 121665 * e));
        }

        ConditionalSwap(ref x2, ref x3, swap);
        ConditionalSwap(ref z2, ref z3, swap);
        BigInteger result = Mod(x2 * BigInteger.ModPow(z2, P - 2, P));
        return ToLittleEndian(result, 32);
    }

    private static byte[] Stream(int length, byte[] nonce, byte[] key)
    {
        return XorStream(new byte[length], nonce, key);
    }

    private static byte[] XorStream(byte[] message, byte[] nonce, byte[] key)
    {
        if (nonce.Length != 24 || key.Length != 32)
        {
            throw new ArgumentException("XSalsa20 requires a 24-byte nonce and a 32-byte key.");
        }

        byte[] subKey = HSalsa20(nonce.AsSpan(0, 16).ToArray(), key);
        byte[] salsaNonce = new byte[8];
        Buffer.BlockCopy(nonce, 16, salsaNonce, 0, 8);

        byte[] output = new byte[message.Length];
        byte[] blockInput = new byte[16];
        Buffer.BlockCopy(salsaNonce, 0, blockInput, 0, 8);
        int offset = 0;

        while (offset < message.Length)
        {
            byte[] block = Salsa20(blockInput, subKey);
            int count = Math.Min(64, message.Length - offset);
            for (int i = 0; i < count; i++)
            {
                output[offset + i] = (byte)(message[offset + i] ^ block[i]);
            }

            IncrementCounter(blockInput);
            offset += count;
        }

        return output;
    }

    private static byte[] HSalsa20(byte[] input, byte[] key)
    {
        uint[] x = Core(input, key);
        byte[] output = new byte[32];
        Store(output, 0, x[0]);
        Store(output, 4, x[5]);
        Store(output, 8, x[10]);
        Store(output, 12, x[15]);
        Store(output, 16, x[6]);
        Store(output, 20, x[7]);
        Store(output, 24, x[8]);
        Store(output, 28, x[9]);
        return output;
    }

    private static byte[] Salsa20(byte[] input, byte[] key)
    {
        uint[] x = Core(input, key);
        uint[] original = InitialState(input, key);
        byte[] output = new byte[64];

        for (int i = 0; i < 16; i++)
        {
            Store(output, i * 4, unchecked(x[i] + original[i]));
        }

        return output;
    }

    private static uint[] Core(byte[] input, byte[] key)
    {
        uint[] x = InitialState(input, key);

        for (int i = 0; i < 10; i++)
        {
            QuarterRound(ref x[0], ref x[4], ref x[8], ref x[12]);
            QuarterRound(ref x[5], ref x[9], ref x[13], ref x[1]);
            QuarterRound(ref x[10], ref x[14], ref x[2], ref x[6]);
            QuarterRound(ref x[15], ref x[3], ref x[7], ref x[11]);
            QuarterRound(ref x[0], ref x[1], ref x[2], ref x[3]);
            QuarterRound(ref x[5], ref x[6], ref x[7], ref x[4]);
            QuarterRound(ref x[10], ref x[11], ref x[8], ref x[9]);
            QuarterRound(ref x[15], ref x[12], ref x[13], ref x[14]);
        }

        return x;
    }

    private static uint[] InitialState(byte[] input, byte[] key)
    {
        return
        [
            Load(Sigma, 0),
            Load(key, 0),
            Load(key, 4),
            Load(key, 8),
            Load(key, 12),
            Load(Sigma, 4),
            Load(input, 0),
            Load(input, 4),
            Load(input, 8),
            Load(input, 12),
            Load(Sigma, 8),
            Load(key, 16),
            Load(key, 20),
            Load(key, 24),
            Load(key, 28),
            Load(Sigma, 12)
        ];
    }

    private static byte[] Poly1305(byte[] message, byte[] key)
    {
        byte[] rBytes = key.AsSpan(0, 16).ToArray();
        rBytes[3] &= 15;
        rBytes[4] &= 252;
        rBytes[7] &= 15;
        rBytes[8] &= 252;
        rBytes[11] &= 15;
        rBytes[12] &= 252;
        rBytes[15] &= 15;

        BigInteger r = FromLittleEndian(rBytes);
        BigInteger s = FromLittleEndian(key.AsSpan(16, 16).ToArray());
        BigInteger p = (BigInteger.One << 130) - 5;
        BigInteger h = BigInteger.Zero;

        for (int offset = 0; offset < message.Length; offset += 16)
        {
            int count = Math.Min(16, message.Length - offset);
            byte[] block = new byte[count + 1];
            Buffer.BlockCopy(message, offset, block, 0, count);
            block[count] = 1;
            h = ((h + FromLittleEndian(block)) * r) % p;
        }

        BigInteger tag = (h + s) & ((BigInteger.One << 128) - 1);
        return ToLittleEndian(tag, 16);
    }

    private static uint Load(byte[] data, int offset)
    {
        return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
    }

    private static void Store(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }

    private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
    {
        b ^= BitOperations.RotateLeft(unchecked(a + d), 7);
        c ^= BitOperations.RotateLeft(unchecked(b + a), 9);
        d ^= BitOperations.RotateLeft(unchecked(c + b), 13);
        a ^= BitOperations.RotateLeft(unchecked(d + c), 18);
    }

    private static void IncrementCounter(byte[] input)
    {
        uint carry = 1;
        for (int i = 8; i < 16; i++)
        {
            carry += input[i];
            input[i] = (byte)carry;
            carry >>= 8;
        }
    }

    private static BigInteger FromLittleEndian(byte[] bytes)
    {
        return new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
    }

    private static byte[] ToLittleEndian(BigInteger value, int length)
    {
        byte[] output = new byte[length];
        byte[] bytes = value.ToByteArray(isUnsigned: true, isBigEndian: false);
        Buffer.BlockCopy(bytes, 0, output, 0, Math.Min(length, bytes.Length));
        return output;
    }

    private static BigInteger Mod(BigInteger value)
    {
        value %= P;
        return value.Sign < 0 ? value + P : value;
    }

    private static void ConditionalSwap(ref BigInteger a, ref BigInteger b, int swap)
    {
        if (swap == 0)
        {
            return;
        }

        (a, b) = (b, a);
    }
}
