using System.Drawing;
using Moq;
using SCCapture;

namespace Cstone.Capture.Tests;

public class SignatureTests {
    private readonly Mock<IImageUtils> _mockImageUtils;
    private static CaptureProfile _locationProfile = new CaptureProfile
    {
        Width = 400,
        Height = 42,
        X = 2450,
        Y = 80,
        MinSamples = 10,
        MinConfidence = 0.7f,
        ShipModel = ShipModel.Vulture,
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX") ?? "C:/Users/JaniHyytiäinen/src/Cstone.Capture/tessdata"
    };
    private static CaptureProfile _signatureProfile = new CaptureProfile
    {
        Width = 160,
        Height = 160,
        // Mole         1010
        // Vanguard     935
        X = 1060,
        // Mole         550
        // Vanguard     410
        Y = 490,
        MinSamples = 10,
        MinConfidence = 0.7f,        
        ShipModel = ShipModel.Vulture,
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX") ?? "C:/Users/JaniHyytiäinen/src/Cstone.Capture/tessdata"
    };

    public SignatureTests()
    {
        _mockImageUtils = new Mock<IImageUtils>();
        var original = Bitmap.FromFile("Signature.png");
        var image = new ImageUtils().ProcessImage(original, _signatureProfile);
        _mockImageUtils.Setup(svc => svc.GetScreenshot(_signatureProfile)).Returns(original);
        _mockImageUtils.Setup(svc => svc.ProcessImage(original, _signatureProfile)).Returns(image);
    }

    [Fact]
    public void Should_Get_Signature() {
        // Arrange
        var sut = new CaptureUtils(_locationProfile, _signatureProfile, _mockImageUtils.Object);
        sut.Scanning();

        // Act
        for (int i = 0; i < 5; i++)
        {
            sut.CaptureSignature();
        }
        var actual = sut.GetSignature();

        // Assert
        Assert.Equal(0.5f, actual.Confidence);
        Assert.Equal("1720", actual.Label);
        Assert.Equal(5, actual.Count);
        Assert.Equal(5160, actual.Value);

        Assert.Equal(CaptureMode.Scanning, sut.Mode);
    }

    [Fact]
    public void Should_Return_To_StandBy() {
        // Arrange
        var sut = new CaptureUtils(_locationProfile, _signatureProfile, _mockImageUtils.Object);
        sut.Scanning();

        // Act
        for (int i = 0; i < _signatureProfile.MinSamples; i++)
        {
            sut.CaptureSignature();
        }
        var actual = sut.GetSignature();

        // Assert
        Assert.Equal(1, actual.Confidence);
        Assert.Equal(CaptureMode.StandBy, sut.Mode);
    }
}