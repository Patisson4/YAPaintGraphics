using YAPaint.Models;

namespace YAPaint.Tools;

public class ImageScaler
{
    public PortableBitmap ScaleImageNearestNeighbor(PortableBitmap portableBitmap, int newWidth, int newHeight,
        double offsetX,
        double offsetY)
    {
        var scaledPortableBitmap = new PortableBitmap(new ColorSpace[newWidth, newHeight],
            portableBitmap.ColorConverter,
            true, true, true);
        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                double sourceX = (x - offsetX) * portableBitmap.Width / newWidth;
                double sourceY = (y - offsetY) * portableBitmap.Height / newHeight;

                int sourceXInt = (int)double.Round(sourceX);
                int sourceYInt = (int)double.Round(sourceY);

                if (sourceXInt < 0)
                {
                    sourceXInt = 0;
                }

                if (sourceXInt >= portableBitmap.Width)
                {
                    sourceXInt = portableBitmap.Width - 1;
                }

                if (sourceYInt < 0)
                {
                    sourceYInt = 0;
                }

                if (sourceYInt >= portableBitmap.Height)
                {
                    sourceYInt = portableBitmap.Height - 1;
                }

                scaledPortableBitmap.SetPixel(x, y, portableBitmap.GetPixel(sourceXInt, sourceYInt));
            }
        }

        return scaledPortableBitmap;
    }
    
    
}