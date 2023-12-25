public struct ConfidenceResult {
    public ConfidenceResult(string label, int value, int count, float confidence)
    {
        Label = label;
        Value = value;
        Count = count;
        Confidence = confidence;
    }
    public string Label { get; private set; } = string.Empty;
    public int Value { get; private set; } = 0;

    public int Count { get; private set; } = 0;
    public float Confidence { get; set; } = 0;
}