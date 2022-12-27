using System;
using System.IO;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class ImageReader
{
    //ArrayPool returns more memory than necessary, therefore SequenceEqual's length check fails  
    private static readonly byte[] Signature = new byte[8];

    public static ColorSpace[,] ReadImage(Stream stream)
    {
        byte firstByte = (byte)stream.ReadByte();
        if (firstByte == "P"u8[0])
        {
            return PnmParser.ReadImage(stream);
        }

        Signature[0] = firstByte;
        if (stream.Read(Signature, 1, 7) == 7 && PngConverter.IsValidSignature(Signature))
        {
            return PngConverter.ReadPng(stream);
        }

        throw new NotSupportedException("Unknown format specification");
    }
}
