using System.Drawing;
using Moq;
using CStone;
using CStone.Types;

namespace Cstone.Cli.Tests;

public class LocationTests {
    private readonly Mock<IImageUtils> _mockImageUtils;
    private static CaptureProfile _locationProfile = new()
    {
        Width = 400,
        Height = 42,
        X = 2450,
        Y = 80,
        MinSamples = 10,
        MinConfidence = 0.7f,
        ShipModel = ShipModel.Vulture,
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX") ?? "C:/Users/JaniHyytiäinen/src/Cstone.Cli/tessdata"
    };
    private static CaptureProfile _signatureProfile = new()
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
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX") ?? "C:/Users/JaniHyytiäinen/src/Cstone.Cli/tessdata"
    };

    public LocationTests()
    {
        _mockImageUtils = new Mock<IImageUtils>();
        var original = Bitmap.FromFile("Location.png");
        var image = new ImageUtils().ProcessImage(original, _locationProfile);
        _mockImageUtils.Setup(svc => svc.GetScreenshot(_locationProfile)).Returns(original);
        _mockImageUtils.Setup(svc => svc.ProcessImage(original, _locationProfile)).Returns(image);
    }

    [Fact]
    public void Should_Get_Location() {
        // Arrange
        var sut = new CaptureUtils("EU", _mockImageUtils.Object);
        sut.LocationMode(_locationProfile, "ARC L1", "CRU L5");

        // Act
        for (int i = 0; i < 5; i++)
        {
            sut.CaptureLocation();
        }
        var actual = sut.GetLocationResult();

        // Assert
        Assert.Equal(0.5f, actual.Confidence);
        Assert.Equal("Stanton", actual.Label);
        Assert.Equal(5, actual.Count);
        Assert.Equal(20475038, actual.Value);

        Assert.Equal(CaptureMode.Location, sut.Mode);
    }

    [Fact]
    public void Should_Return_To_StandBy() {
        // Arrange
        var sut = new CaptureUtils("EU", _mockImageUtils.Object);
        sut.LocationMode(_locationProfile, "ARC L1", "CRU L5");

        // Act
        for (int i = 0; i < 10; i++)
        {
            sut.CaptureLocation();
        }
        var actual = sut.GetLocationResult();

        // Assert
        Assert.Equal(1, actual.Confidence);
        Assert.Equal(CaptureMode.StandBy, sut.Mode);
    }
}