using System.Drawing;

namespace SCCapture;

public interface IImageUtils
{
    Image ProcessImage(Image captureBitmap, CaptureProfile profile);
    Image GetScreenshot(CaptureProfile profile);
    Image RotateImage(Image image, float angle);
}