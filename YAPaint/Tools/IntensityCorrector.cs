using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public class IntensityCorrector
{
    public static PortableBitmap CorrectIntensity(PortableBitmap bitmap, double ignoreProportion, double[][] histograms)
    {
        var currentBack = 0d;
        int i;
        var threshold = ignoreProportion * bitmap.Width * bitmap.Height * 3;
        for (i = 0; i < 256; i++)
        {
            for (var c = 0; c < 3; c++)
            {
                currentBack += histograms[c][i];
            }
            if (currentBack >= threshold)
                break;
        }

        if (i >= 256)
            i = 255;
        double leftEdge = i / 255d;
        Console.WriteLine("leftedge:" + leftEdge);
        
        currentBack = threshold;
        for (i = 255; i >= 0; i--)
        {
            for (int c = 0; c < 3; c++)
            {
                currentBack -= histograms[c][i];
            }
            if (currentBack <= 0)
                break;
        }

        if (i < 0)
            i = 0;
        double rightEdge = i / 255d;
        Console.WriteLine("rightedge:" + rightEdge);
        
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                var newPixelFirst = Coefficient.Normalize(Coefficient.Denormalize((float)Math.Clamp((pixel.First - leftEdge) / (rightEdge - leftEdge), 0d, 1.0d)));
                var newPixelSecond = Coefficient.Normalize(Coefficient.Denormalize((float)Math.Clamp((pixel.Second - leftEdge) / (rightEdge - leftEdge), 0d,
                    1.0d)));
                var newPixelThird = Coefficient.Normalize(Coefficient.Denormalize((float)Math.Clamp((pixel.Third - leftEdge) / (rightEdge - leftEdge), 0d, 1.0d)));
                var newPixel = new ColorSpace(newPixelFirst, newPixelSecond, newPixelThird);
                bitmap.SetPixel(x, y, newPixel);
            }
        }

        return bitmap;
    }


}