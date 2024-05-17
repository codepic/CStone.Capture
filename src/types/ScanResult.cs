public struct ScanResult {
    public string Server { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public int Distance { get; set; }
    public string Archetype { get; internal set; }
    public int Signature { get; set; }
    public int Quantity { get; set; }
    public float Confidence { get; set; }
}