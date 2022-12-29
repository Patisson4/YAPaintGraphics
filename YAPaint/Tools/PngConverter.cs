﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using YAPaint.Models;
using YAPaint.Models.ColorSpaces;

namespace YAPaint.Tools;

public static class PngConverter
{
    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
    private static readonly byte[] IendChunk = { 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 };
    private static readonly byte[] IhdrBuffer = new byte[13];
    private static readonly byte[] SignatureBuffer = new byte[8];

    private static readonly byte[] IhdrChunkName = "IHDR"u8.ToArray();
    private static readonly byte[] GamaChunkName = "gAMA"u8.ToArray();
    private static readonly byte[] PlteChunkName = "PLTE"u8.ToArray();
    private static readonly byte[] IdatChunkName = "IDAT"u8.ToArray();
    private static readonly byte[] IendChunkName = "IEND"u8.ToArray();

    private const int SizeOfInt = sizeof(int);

    public static void WritePng(this PortableBitmap bitmap, Stream outputStream, float gamma)
    {
        outputStream.Write(PngSignature, 0, PngSignature.Length);
        WriteIhdrChunk(bitmap, outputStream);
        WriteGamaChunk(outputStream, gamma);
        WriteIdatChunk(bitmap, outputStream);
        WriteIendChunk(outputStream);
    }

    public static ColorSpace[,] ReadPng(Stream inputStream, out float gamma)
    {
        int width = 0;
        int height = 0;
        byte bitDepth = 0;
        byte colorType = 0;
        gamma = -1;
        using var pixelData = new MemoryStream();
        var palette = new List<(byte, byte, byte)>();

        if (inputStream.Read(SignatureBuffer, 0, 8) != 8 || !SignatureBuffer.SequenceEqual(PngSignature))
        {
            throw new InvalidDataException("Invalid Png Signature");
        }

        while (true)
        {
            byte[] chunkLengthBytes = new byte[4];
            inputStream.Read(chunkLengthBytes, 0, 4);
            int chunkLength = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(chunkLengthBytes, 0));

            byte[] chunkTypeBytes = new byte[4];
            inputStream.Read(chunkTypeBytes, 0, 4);

            byte[] chunkData = new byte[chunkLength];
            inputStream.Read(chunkData, 0, chunkLength);

            byte[] chunkCrcBytes = new byte[4];
            inputStream.Read(chunkCrcBytes, 0, 4);
            uint chunkCrc = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(chunkCrcBytes, 0));

            uint expectedChunkCrc = CalculateCrc(chunkTypeBytes, chunkData);
            if (chunkCrc != expectedChunkCrc)
            {
                throw new InvalidDataException("Invalid crc");
            }

            if (chunkTypeBytes.SequenceEqual(IhdrChunkName))
            {
                width = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(chunkData, 0));
                height = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(chunkData, 4));
                bitDepth = chunkData[8];
                colorType = chunkData[9];
                if (chunkData[12] == 1)
                {
                    throw new NotSupportedException("Interlacing method Adam7 is not supported");
                }
            }
            else if (chunkTypeBytes.SequenceEqual(GamaChunkName))
            {
                var parsedGamma = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(chunkData));
                if (parsedGamma is 45454 or 45455 or 45456)
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
                    throw new InvalidDataException("Invalid PLTE fromat");
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
                MyFileLogger.Log(
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

                map[x, y] = new ColorSpace(
                    Coefficient.Normalize(r),
                    Coefficient.Normalize(g),
                    Coefficient.Normalize(b));
            }

            raw.CopyTo(prior);
        }

        return map;
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

    private static void WriteIhdrChunk(PortableBitmap bitmap, Stream outputStream)
    {
        BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(bitmap.Width)).CopyTo(IhdrBuffer, 0);
        BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(bitmap.Height)).CopyTo(IhdrBuffer, SizeOfInt);

        IhdrBuffer[8] = 8; // bit depth

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

        outputStream.Write(
            BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness((uint)IhdrBuffer.Length)),
            0,
            SizeOfInt);
        outputStream.Write(IhdrChunkName, 0, IhdrChunkName.Length);
        outputStream.Write(IhdrBuffer, 0, IhdrBuffer.Length);
        outputStream.Write(
            BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(CalculateCrc(IhdrChunkName, IhdrBuffer))),
            0,
            SizeOfInt);
    }

    private static void WriteGamaChunk(Stream outputStream, float gamma)
    {
        uint exactGamma = (uint)(float.Abs(gamma + 1) < float.Epsilon ? 45455 : gamma * 100000);
        byte[] parsedGamma = BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(exactGamma));
        outputStream.Write(
            BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(4)),
            0,
            SizeOfInt);
        outputStream.Write(GamaChunkName, 0, GamaChunkName.Length);
        outputStream.Write(parsedGamma, 0, parsedGamma.Length);
        outputStream.Write(
            BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(CalculateCrc(GamaChunkName, parsedGamma))),
            0,
            SizeOfInt);
    }

    private static void WriteIdatChunk(PortableBitmap bitmap, Stream outputStream)
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

        outputStream.Write(
            BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness((uint)idatData.Length)),
            0,
            SizeOfInt);
        outputStream.Write(IdatChunkName, 0, IdatChunkName.Length);
        outputStream.Write(idatData, 0, idatData.Length);
        outputStream.Write(
            BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(CalculateCrc(IdatChunkName, idatData))),
            0,
            SizeOfInt);
    }

    private static void WriteIendChunk(Stream outputStream)
    {
        outputStream.Write(IendChunk, 0, IendChunk.Length);
    }

    private static uint CalculateCrc(byte[] chunkType, byte[] chunkData)
    {
        uint crc = chunkType.Aggregate(0xffffffff, UpdateCrc);
        crc = chunkData.Aggregate(crc, UpdateCrc);

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