using System;
using System.Buffers.Binary;
using System.Numerics;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Managed MD5 used to derive identicons where the platform implementation is unavailable,
/// such as WebAssembly. It is not intended for security purposes.
/// </summary>
internal static class ManagedMd5
{
    private const int BlockSize = 64;

    private static readonly int[] Shifts =
    [
        7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22,
        5, 9, 14, 20, 5, 9, 14, 20, 5, 9, 14, 20, 5, 9, 14, 20,
        4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23,
        6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21,
    ];

    private static readonly uint[] Constants =
    [
        0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
        0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
        0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
        0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
        0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
        0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
        0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
        0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391,
    ];

    public static byte[] HashData(ReadOnlySpan<byte> source)
    {
        var paddedLength = ((source.Length + 8) / BlockSize + 1) * BlockSize;
        var buffer = new byte[paddedLength];
        source.CopyTo(buffer);
        buffer[source.Length] = 0x80;
        BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(paddedLength - 8), (ulong)source.Length * 8);

        var a0 = 0x67452301u;
        var b0 = 0xefcdab89u;
        var c0 = 0x98badcfeu;
        var d0 = 0x10325476u;
        Span<uint> words = stackalloc uint[16];

        for (var offset = 0; offset < paddedLength; offset += BlockSize)
        {
            for (var index = 0; index < words.Length; index++)
            {
                words[index] = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset + index * 4));
            }

            var a = a0;
            var b = b0;
            var c = c0;
            var d = d0;

            for (var round = 0; round < BlockSize; round++)
            {
                uint mix;
                int word;

                if (round < 16)
                {
                    mix = (b & c) | (~b & d);
                    word = round;
                }
                else if (round < 32)
                {
                    mix = (d & b) | (~d & c);
                    word = (5 * round + 1) % 16;
                }
                else if (round < 48)
                {
                    mix = b ^ c ^ d;
                    word = (3 * round + 5) % 16;
                }
                else
                {
                    mix = c ^ (b | ~d);
                    word = 7 * round % 16;
                }

                mix += a + Constants[round] + words[word];
                a = d;
                d = c;
                c = b;
                b += BitOperations.RotateLeft(mix, Shifts[round]);
            }

            a0 += a;
            b0 += b;
            c0 += c;
            d0 += d;
        }

        var hash = new byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(hash.AsSpan(0), a0);
        BinaryPrimitives.WriteUInt32LittleEndian(hash.AsSpan(4), b0);
        BinaryPrimitives.WriteUInt32LittleEndian(hash.AsSpan(8), c0);
        BinaryPrimitives.WriteUInt32LittleEndian(hash.AsSpan(12), d0);
        return hash;
    }
}
