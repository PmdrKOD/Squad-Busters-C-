using System.Buffers.Binary;
using System.Numerics;

namespace ReversedOfClans.Cryptography;

internal static class Blake2b
{
    private static readonly ulong[] Iv =
    [
        0x6a09e667f3bcc908UL, 0xbb67ae8584caa73bUL,
        0x3c6ef372fe94f82bUL, 0xa54ff53a5f1d36f1UL,
        0x510e527fade682d1UL, 0x9b05688c2b3e6c1fUL,
        0x1f83d9abfb41bd6bUL, 0x5be0cd19137e2179UL
    ];

    private static readonly byte[,] Sigma =
    {
        { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
        { 14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3 },
        { 11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4 },
        { 7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8 },
        { 9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13 },
        { 2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9 },
        { 12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11 },
        { 13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10 },
        { 6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5 },
        { 10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0 },
        { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
        { 14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3 }
    };

    public static byte[] Hash(ReadOnlySpan<byte> data, int digestLength)
    {
        if (digestLength is < 1 or > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(digestLength));
        }

        ulong[] h = Iv.ToArray();
        h[0] ^= 0x01010000UL ^ (uint)digestLength;

        ulong counter = 0;
        int offset = 0;
        while (data.Length - offset > 128)
        {
            counter += 128;
            Compress(h, data.Slice(offset, 128), counter, false);
            offset += 128;
        }

        Span<byte> block = stackalloc byte[128];
        data[offset..].CopyTo(block);
        counter += (ulong)(data.Length - offset);
        Compress(h, block, counter, true);

        byte[] full = new byte[64];
        for (int i = 0; i < h.Length; i++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(full.AsSpan(i * 8, 8), h[i]);
        }

        return full[..digestLength];
    }

    private static void Compress(ulong[] h, ReadOnlySpan<byte> block, ulong counter, bool isLast)
    {
        Span<ulong> m = stackalloc ulong[16];
        Span<ulong> v = stackalloc ulong[16];

        for (int i = 0; i < 16; i++)
        {
            m[i] = BinaryPrimitives.ReadUInt64LittleEndian(block.Slice(i * 8, 8));
        }

        for (int i = 0; i < 8; i++)
        {
            v[i] = h[i];
            v[i + 8] = Iv[i];
        }

        v[12] ^= counter;
        if (isLast)
        {
            v[14] = ~v[14];
        }

        for (int round = 0; round < 12; round++)
        {
            Mix(v, m, round, 0, 0, 4, 8, 12);
            Mix(v, m, round, 2, 1, 5, 9, 13);
            Mix(v, m, round, 4, 2, 6, 10, 14);
            Mix(v, m, round, 6, 3, 7, 11, 15);
            Mix(v, m, round, 8, 0, 5, 10, 15);
            Mix(v, m, round, 10, 1, 6, 11, 12);
            Mix(v, m, round, 12, 2, 7, 8, 13);
            Mix(v, m, round, 14, 3, 4, 9, 14);
        }

        for (int i = 0; i < 8; i++)
        {
            h[i] ^= v[i] ^ v[i + 8];
        }
    }

    private static void Mix(Span<ulong> v, Span<ulong> m, int round, int sigmaIndex, int a, int b, int c, int d)
    {
        ulong x = m[Sigma[round, sigmaIndex]];
        ulong y = m[Sigma[round, sigmaIndex + 1]];

        v[a] = v[a] + v[b] + x;
        v[d] = BitOperations.RotateRight(v[d] ^ v[a], 32);
        v[c] += v[d];
        v[b] = BitOperations.RotateRight(v[b] ^ v[c], 24);
        v[a] = v[a] + v[b] + y;
        v[d] = BitOperations.RotateRight(v[d] ^ v[a], 16);
        v[c] += v[d];
        v[b] = BitOperations.RotateRight(v[b] ^ v[c], 63);
    }
}
