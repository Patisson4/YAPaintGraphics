using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using YAPaint.Models;
using YAPaint.Models.ColorSpaces;
using YAPaint.Models.ExtraColorSpaces;

namespace YAPaint.Tools;

public static class PngConverter
{
    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
    private static readonly byte[] IhdrBuffer = new byte[13];
    private static readonly byte[] IendChunk = { 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 };

    private static readonly byte[] IhdrChunkName = "IHDR"u8.ToArray();
    private static readonly byte[] GamaChunkName = "gAMA"u8.ToArray();
    private static readonly byte[] PlteChunkName = "PLTE"u8.ToArray();
    private static readonly byte[] IdatChunkName = "IDAT"u8.ToArray();
    private static readonly byte[] IendChunkName = "IEND"u8.ToArray();

    private const int SizeOfInt = sizeof(int);
    private const int SrgbGamma = 45455;

    public static void WritePng(this PortableBitmap bitmap, Stream outputStream)
    {
        outputStream.Write(PngSignature, 0, PngSignature.Length);
        WriteIhdrChunk(outputStream, bitmap);
        WriteGamaChunk(outputStream, bitmap.Gamma);
        WriteIdatChunk(outputStream, bitmap);
        WriteIendChunk(outputStream);
    }

    public static PortableBitmap ReadPng(Stream inputStream)
    {
        int width = 0;
        int height = 0;
        byte bitDepth = 0;
        byte colorType = 0;
        float gamma = -1;
        using var pixelData = new MemoryStream();
        var palette = new List<(byte, byte, byte)>();

        Span<byte> signature = stackalloc byte[PngSignature.Length];

        if (inputStream.Read(signature) != 8 || !signature.SequenceEqual(PngSignature))
        {
            throw new InvalidDataException("Invalid Png Signature");
        }

        Span<byte> chunkLengthBytes = stackalloc byte[SizeOfInt];
        Span<byte> chunkTypeBytes = stackalloc byte[SizeOfInt];

        Span<byte> chunkCrcBytes = stackalloc byte[SizeOfInt];
        while (true)
        {
            inputStream.Read(chunkLengthBytes);
            inputStream.Read(chunkTypeBytes);

            int chunkLength = BinaryPrimitives.ReadInt32BigEndian(chunkLengthBytes);
            byte[] chunkData = new byte[chunkLength]; // any way to improve memory allocations here???

            inputStream.Read(chunkData, 0, chunkLength);
            inputStream.Read(chunkCrcBytes);

            uint chunkCrc = BinaryPrimitives.ReadUInt32BigEndian(chunkCrcBytes);

            uint expectedChunkCrc = CalculateCrc(chunkTypeBytes, chunkData);
            if (chunkCrc != expectedChunkCrc)
            {
                throw new InvalidDataException("Invalid crc");
            }

            if (chunkTypeBytes.SequenceEqual(IhdrChunkName))
            {
                width = BinaryPrimitives.ReadInt32BigEndian(chunkData);
                height = BinaryPrimitives.ReadInt32BigEndian(chunkData.AsSpan()[SizeOfInt..]);
                bitDepth = chunkData[8];
                colorType = chunkData[9];
                if (chunkData[12] != 0)
                {
                    throw new NotSupportedException("Interlacing methods are not supported");
                }
            }
            else if (chunkTypeBytes.SequenceEqual(GamaChunkName))
            {
                int parsedGamma = BinaryPrimitives.ReadInt32BigEndian(chunkData);
                if (parsedGamma is SrgbGamma)
                {
                    gamma = 0;
                }
                else
                {
                    gamma = (float)parsedGamma / 100000;
                }
            }
            else if (chunkTypeBytes.SequenceEqual(PlteChunkName))
            {
                if (chunkLength % 3 != 0)
                {
                    throw new InvalidDataException("Invalid PLTE format");
                }

                for (int i = 0; i < chunkLength; i += 3)
                {
                    palette.Add((chunkData[i], chunkData[i + 1], chunkData[i + 2]));
                }
            }
            else if (chunkTypeBytes.SequenceEqual(IdatChunkName))
            {
                pixelData.Write(chunkData);
            }
            else if (chunkTypeBytes.SequenceEqual(IendChunkName))
            {
                break;
            }
            else
            {
                FileLogger.Log(
                    "WRN",
                    $"Unsupported chunk format: {Encoding.Default.GetString(chunkTypeBytes)}; chunk ignored");
            }
        }

        if (width == 0 || height == 0)
        {
            throw new InvalidDataException("Missing required chunk in PNG file");
        }

        if (bitDepth != 8)
        {
            throw new NotSupportedException("Unsupported bit depth in PNG file");
        }

        pixelData.Position = 0;
        using var decompressedData = new MemoryStream();
        using (var deflateStream = new ZLibStream(pixelData, CompressionMode.Decompress, true))
        {
            deflateStream.CopyTo(decompressedData);
        }

        decompressedData.Position = 0;
        var bytesPerPixel = BytesPerPixel(colorType);

        Span<byte> prior = stackalloc byte[width * bytesPerPixel];
        Span<byte> raw = stackalloc byte[width * bytesPerPixel];
        var map = new ColorSpace[width, height];

        prior.Clear();

        for (int y = 0; y < height; y++)
        {
            byte filterType = (byte)decompressedData.ReadByte();
            decompressedData.Read(raw);

            switch (filterType)
            {
                case 0: // None
                    break;
                case 1: // Sub
                    for (int x = bytesPerPixel; x < raw.Length; x++)
                    {
                        raw[x] += raw[x - bytesPerPixel];
                    }

                    break;
                case 2: // Up
                    for (int x = 0; x < raw.Length; x++)
                    {
                        raw[x] += prior[x];
                    }

                    break;
                case 3: // Average
                    for (int x = 0; x < bytesPerPixel; x++)
                    {
                        raw[x] += (byte)(prior[x] / 2);
                    }

                    for (int x = bytesPerPixel; x < raw.Length; x++)
                    {
                        raw[x] += (byte)((raw[x - bytesPerPixel] + prior[x]) / 2);
                    }

                    break;
                case 4: // Paeth
                    for (int x = 0; x < bytesPerPixel; x++)
                    {
                        raw[x] += (byte)PaethPredictor(0, prior[x], 0);
                    }

                    for (int x = bytesPerPixel; x < raw.Length; x++)
                    {
                        raw[x] += (byte)PaethPredictor(raw[x - bytesPerPixel], prior[x], prior[x - bytesPerPixel]);
                    }

                    break;
                default:
                    throw new InvalidDataException("Invalid filter type");
            }

            for (int x = 0; x < width; x++)
            {
                byte r, g, b;
                if (colorType is 3)
                {
                    (r, g, b) = palette[raw[x]];
                }
                else
                {
                    r = raw[x * bytesPerPixel];
                    if (colorType is 0 or 4)
                    {
                        g = r;
                        b = r;
                    }
                    else // color type 2 or 6
                    {
                        g = raw[x * bytesPerPixel + 1];
                        b = raw[x * bytesPerPixel + 2];
                        // scanline[x * bytesPerPixel + 3] - alpha - skipped
                    }
                }

                map[x, y] = new ColorSpace
                {
                    First = Coefficient.Normalize(r),
                    Second = Coefficient.Normalize(g),
                    Third = Coefficient.Normalize(b),
                };
            }

            raw.CopyTo(prior);
        }

        return new PortableBitmap(map, Rgb.Instance, gamma);
    }

    private static byte BytesPerPixel(byte colorType)
    {
        return colorType switch
        {
            0 => 1,
            2 => 3,
            3 => 1,
            4 => 2,
            6 => 4,
            _ => throw new InvalidDataException(),
        };
    }

    private static int PaethPredictor(int a, int b, int c)
    {
        int p = a + b - c;
        int pa = int.Abs(p - a);
        int pb = int.Abs(p - b);
        int pc = int.Abs(p - c);

        if (pa <= pb && pa <= pc)
        {
            return a;
        }

        return pb <= pc ? b : c;
    }

    private static void WriteIhdrChunk(Stream outputStream, PortableBitmap bitmap)
    {
        BinaryPrimitives.WriteInt32BigEndian(IhdrBuffer.AsSpan()[..SizeOfInt], bitmap.Width);
        BinaryPrimitives.WriteInt32BigEndian(IhdrBuffer.AsSpan()[SizeOfInt..(SizeOfInt * 2)], bitmap.Height);

        IhdrBuffer[8] = 8; // bit depth

        // TODO: Replace with IsGrayScaleImage or sth
        if (bitmap.ColorConverter is BlackAndWhite or GreyScale
         || bitmap.IsFirstVisible && !bitmap.IsSecondVisible && !bitmap.IsThirdVisible
         || !bitmap.IsFirstVisible && bitmap.IsSecondVisible && !bitmap.IsThirdVisible
         || !bitmap.IsFirstVisible && !bitmap.IsSecondVisible && bitmap.IsThirdVisible)
        {
            IhdrBuffer[9] = 0; // greyscale
        }
        else
        {
            IhdrBuffer[9] = 2; // truecolor
        }

        IhdrBuffer[10] = 0; // compression method (deflate)
        IhdrBuffer[11] = 0; // filter method (adaptive)
        IhdrBuffer[12] = 0; // interlace method (none)

        var chunk = new Chunk(IhdrChunkName, IhdrBuffer);
        outputStream.WriteChunk(ref chunk);
    }

    private static void WriteGamaChunk(Stream outputStream, float gamma)
    {
        int exactGamma = (int)(float.Abs(gamma + 1) < float.Epsilon ? SrgbGamma : gamma * 100000);
        Span<byte> parsedGamma = stackalloc byte[SizeOfInt];
        BinaryPrimitives.WriteInt32BigEndian(parsedGamma, exactGamma);

        var chunk = new Chunk(GamaChunkName, parsedGamma);
        outputStream.WriteChunk(ref chunk);
    }

    private static void WriteIdatChunk(Stream outputStream, PortableBitmap bitmap)
    {
        using var pixelDataStream = new MemoryStream();
        for (int y = 0; y < bitmap.Height; y++)
        {
            pixelDataStream.WriteByte(0); // filter type (none)
            for (int x = 0; x < bitmap.Width; x++)
            {
                ColorSpace color = bitmap.GetPixel(x, y);
                pixelDataStream.WriteByte(Coefficient.Denormalize(color.First));

                if (IhdrBuffer[9] == 0)
                {
                    continue;
                }

                pixelDataStream.WriteByte(Coefficient.Denormalize(color.Second));
                pixelDataStream.WriteByte(Coefficient.Denormalize(color.Third));
            }
        }

        using var compressedData = new MemoryStream();
        pixelDataStream.Position = 0;

        using (var deflateStream = new ZLibStream(compressedData, CompressionLevel.Optimal, true))
        {
            pixelDataStream.CopyTo(deflateStream);
        }

        byte[] idatData = compressedData.ToArray();
        var chunk = new Chunk(IdatChunkName, idatData);
        outputStream.WriteChunk(ref chunk);
    }

    private static void WriteIendChunk(Stream outputStream)
    {
        outputStream.Write(IendChunk, 0, IendChunk.Length);
    }

    private static void WriteChunk(this Stream outputStream, ref Chunk chunk)
    {
        Span<byte> chunkLength = stackalloc byte[SizeOfInt];
        BinaryPrimitives.WriteInt32BigEndian(chunkLength, chunk.Data.Length);

        Span<byte> chunkCrc = stackalloc byte[SizeOfInt];
        BinaryPrimitives.WriteUInt32BigEndian(chunkCrc, CalculateCrc(chunk.Type, chunk.Data));

        outputStream.Write(chunkLength);
        outputStream.Write(chunk.Type);
        outputStream.Write(chunk.Data);
        outputStream.Write(chunkCrc);
    }

    private readonly ref struct Chunk
    {
        public ReadOnlySpan<byte> Type { get; }
        public ReadOnlySpan<byte> Data { get; }

        public Chunk(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
        {
            Type = type;
            Data = data;
        }
    }

    private static uint CalculateCrc(ReadOnlySpan<byte> chunkType, ReadOnlySpan<byte> chunkData)
    {
        uint crc = 0xffffffff;

        foreach (byte b in chunkType)
        {
            crc = UpdateCrc(crc, b);
        }

        foreach (byte b in chunkData)
        {
            crc = UpdateCrc(crc, b);
        }

        return crc ^ 0xffffffff;
    }

    private static uint UpdateCrc(uint crc, byte b)
    {
        crc ^= b;
        for (int i = 0; i < 8; i++)
        {
            if ((crc & 1) != 0)
            {
                crc = (crc >> 1) ^ 0xedb88320;
            }
            else
            {
                crc >>= 1;
            }
        }

        return crc;
    }
}
