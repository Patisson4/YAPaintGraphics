using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

    public static void SaveAsPng(this PortableBitmap bitmap, Stream outputStream)
    {
        outputStream.Write(PngSignature, 0, PngSignature.Length);
        WriteIhdrChunk(bitmap, outputStream);
        WriteIdatChunk(bitmap, outputStream);
        WriteIendChunk(outputStream);
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
