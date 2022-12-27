using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class PngConverter
{
    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
    private static readonly byte[] IendChunk = { 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 };
    private static readonly byte[] IhdrBuffer = new byte[13];

    private static readonly byte[] IhdrChunkName = "IHDR"u8.ToArray();
    private static readonly byte[] IdatChunkName = "IDAT"u8.ToArray();
    private static readonly byte[] IendChunkName = "IEND"u8.ToArray();

    private const int SizeOfInt = sizeof(int);

    public static bool IsValidSignature(byte[] signature)
    {
        return signature.SequenceEqual(PngSignature);
    }

    public static void SaveAsPng(this PortableBitmap bitmap, Stream outputStream)
    {
        outputStream.Write(PngSignature, 0, PngSignature.Length);
        WriteIhdrChunk(bitmap, outputStream);
        WriteIdatChunk(bitmap, outputStream);
        WriteIendChunk(outputStream);
    }

    public static ColorSpace[,] ReadPng(Stream inputStream)
    {
        int width = 0;
        int height = 0;
        byte bitDepth = 0;
        byte colorType = 0;
        var pixelData = new List<byte>();

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
            else if (chunkTypeBytes.SequenceEqual(IdatChunkName))
            {
                pixelData.AddRange(chunkData);
            }
            else if (chunkTypeBytes.SequenceEqual(IendChunkName))
            {
                break;
            }
            else
            {
                MyFileLogger.Log("WRN", $"Unsupported chunk format: {Encoding.Default.GetString(chunkTypeBytes)}; chunk ignored");
            }
        }

        if (width == 0 || height == 0 || pixelData == null)
        {
            throw new InvalidDataException("Missing required chunk in PNG file");
        }

        //TODO: support 3
        if (colorType is 3 or 4 or 6)
        {
            throw new NotSupportedException("Unsupported color type in PNG file");
        }

        if (bitDepth != 8)
        {
            throw new NotSupportedException("Unsupported bit depth in PNG file");
        }

        using var decompressedStream = new MemoryStream();

        using (var compressedStream = new MemoryStream())
        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress, true))
        {
            compressedStream.Write(CollectionsMarshal.AsSpan(pixelData));
            compressedStream.Position = 0;
            deflateStream.CopyTo(decompressedStream);
        }

        decompressedStream.Position = 0;
        var map = new ColorSpace[width, height];
        for (int y = 0; y < height; y++)
        {
            //skip filter bit
            decompressedStream.ReadByte();
            for (int x = 0; x < width; x++)
            {
                byte r = (byte)decompressedStream.ReadByte();
                byte g = (byte)decompressedStream.ReadByte();
                byte b = (byte)decompressedStream.ReadByte();
                var color = new ColorSpace(
                    Coefficient.Normalize(r),
                    Coefficient.Normalize(g),
                    Coefficient.Normalize(b));
                map[x, y] = color;
            }
        }

        return map;
    }

    private static void WriteIhdrChunk(PortableBitmap bitmap, Stream outputStream)
    {
        BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(bitmap.Width)).CopyTo(IhdrBuffer, 0);
        BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(bitmap.Height)).CopyTo(IhdrBuffer, SizeOfInt);

        IhdrBuffer[8] = 8; // bit depth
        IhdrBuffer[9] = 2; // color type // TODO: recognize and change color type based on bitmap data
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
                pixelDataStream.WriteByte(Coefficient.Denormalize(color.Second));
                pixelDataStream.WriteByte(Coefficient.Denormalize(color.Third));
            }
        }

        using var dataStream = new MemoryStream();
        pixelDataStream.Position = 0;
        using (var deflateStream = new DeflateStream(dataStream, CompressionLevel.Optimal, true))
        {
            pixelDataStream.CopyTo(deflateStream);
        }

        byte[] idatData = dataStream.ToArray();

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
