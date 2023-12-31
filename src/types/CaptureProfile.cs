namespace CStone.Types;

public struct CaptureProfile {
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int MinSamples { get; set; }
    public float MinConfidence { get; set; }
    public ShipModel ShipModel { get; set; }
    public string TessData { get; set; }
}
