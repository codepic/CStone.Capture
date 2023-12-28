using System.Drawing;
using SCCapture;

namespace Cstone.Capture.Tests;

public class ImageTests {


    public ImageTests()
    {
        
    }

    [Fact]
    public void ExctractColor() {
        var utils = new ImageUtils();
        var bitmap = Bitmap.FromFile("Signature.png");

        var extracted = utils.ExctractColor((Bitmap)bitmap, 0.65f);

        extracted.Save("Duotone.png");
    }
}