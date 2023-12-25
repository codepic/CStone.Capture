using System.Drawing;
using System.Drawing.Imaging;

namespace SCCapture;

public class ImageUtils : IImageUtils
{
    public Image ProcessImage(Image captureBitmap, CaptureProfile profile)
    {
        var resized = new Bitmap(captureBitmap, new Size(captureBitmap.Width * 4, captureBitmap.Height * 4));
        var rotated = RotateImage(resized, -5);

        switch (profile.ShipModel)
        {
            case ShipModel.Mole:
                var filtered = ColorReplace(rotated, 100, Color.LightGray, Color.Black);
                var darkFiltered = ColorReplace(filtered, 100, Color.DarkGray, Color.Black);
                var replaced = ColorReplace(darkFiltered, 50, Color.DarkRed, Color.White);
                return replaced;
            case ShipModel.Vanguard:
                return rotated;
            case ShipModel.Vulture:
                return rotated;
        }


        return rotated;
    }
    public Image GetScreenshot(CaptureProfile profile)
    {
        var captureBitmap = new Bitmap(profile.Width, profile.Height);
        var graphics = Graphics.FromImage(captureBitmap);
        var upperLeftSource = new Point(profile.X, profile.Y);
        var upperLeftDestination = new Point(0, 0);
        graphics.CopyFromScreen(upperLeftSource, upperLeftDestination, captureBitmap.Size);
        return captureBitmap;
    }
    public Image RotateImage(Image bmp, float angle)
    {
        Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
        rotatedImage.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

        using (Graphics g = Graphics.FromImage(rotatedImage))
        {
            // Set the rotation point to the center in the matrix
            g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
            // Rotate
            g.RotateTransform(angle);
            // Restore rotation point in the matrix
            g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
            // Draw the image on the bitmap
            g.DrawImage(bmp, new Point(0, 0));
        }

        return rotatedImage;
    }
    private unsafe Bitmap SetContrast(Bitmap initial, float contrast)
    {

        Bitmap result = (Bitmap)initial.Clone();
        BitmapData data = result.LockBits(
            new Rectangle(0, 0, result.Width, result.Height),
            ImageLockMode.ReadWrite,
            PixelFormat.Format32bppArgb);

        int pixelSize = 4;

        byte[] contrast_lookup = new byte[256];
        double newValue = 0;

        double value1 = Math.Pow(1.0 + contrast / 100.0, 2.0);

        double value2 = (1.0 - value1) * 127.5;

        for (int i = 0; i < 256; i++)
        {
            newValue = (double)i * value1 + value2;
            newValue = newValue < 0.0 ? 0.0 : newValue > 255.0 ? 255.0 : newValue;
            contrast_lookup[i] = (byte)newValue;
        }

        for (int y = 0; y < result.Height; ++y)
        {
            byte* row = (byte*)data.Scan0 + (y * data.Stride);
            for (int x = 0; x < result.Width; ++x)
                for (int i = 0; i < pixelSize; i++)
                    row[x * pixelSize + i] = contrast_lookup[row[x * pixelSize + i]];
        }

        result.UnlockBits(data);
        return result;
    }
    private Image ColorReplace(Image inputImage, int tolerance, Color oldColor, Color NewColor)
    {
        Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height);
        Graphics G = Graphics.FromImage(outputImage);
        G.DrawImage(inputImage, 0, 0);
        for (int y = 0; y < outputImage.Height; y++)
            for (int x = 0; x < outputImage.Width; x++)
            {
                Color PixelColor = outputImage.GetPixel(x, y);
                if (PixelColor.R > oldColor.R - tolerance && PixelColor.R < oldColor.R + tolerance && PixelColor.G > oldColor.G - tolerance && PixelColor.G < oldColor.G + tolerance && PixelColor.B > oldColor.B - tolerance && PixelColor.B < oldColor.B + tolerance)
                {
                    int RColorDiff = oldColor.R - PixelColor.R;
                    int GColorDiff = oldColor.G - PixelColor.G;
                    int BColorDiff = oldColor.B - PixelColor.B;

                    if (PixelColor.R > oldColor.R) RColorDiff = NewColor.R + RColorDiff;
                    else RColorDiff = NewColor.R - RColorDiff;
                    if (RColorDiff > 255) RColorDiff = 255;
                    if (RColorDiff < 0) RColorDiff = 0;
                    if (PixelColor.G > oldColor.G) GColorDiff = NewColor.G + GColorDiff;
                    else GColorDiff = NewColor.G - GColorDiff;
                    if (GColorDiff > 255) GColorDiff = 255;
                    if (GColorDiff < 0) GColorDiff = 0;
                    if (PixelColor.B > oldColor.B) BColorDiff = NewColor.B + BColorDiff;
                    else BColorDiff = NewColor.B - BColorDiff;
                    if (BColorDiff > 255) BColorDiff = 255;
                    if (BColorDiff < 0) BColorDiff = 0;

                    outputImage.SetPixel(x, y, Color.FromArgb(RColorDiff, GColorDiff, BColorDiff));
                }
            }
        return outputImage;
    }

}
