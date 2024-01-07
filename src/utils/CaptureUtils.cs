using System.Drawing;
using System.Drawing.Imaging;
using CStone.Types;
using Newtonsoft.Json;

namespace CStone;
public class CaptureUtils
{
    private CaptureProfile _captureProfile;
    private IList<Tuple<string, int>> _location = new List<Tuple<string, int>>();
    private IList<Tuple<string, int>> _signature = new List<Tuple<string, int>>();
    private string _server;
    private string? _origin;
    private string? _destination;
    private readonly IImageUtils _imageUtils;

    public CaptureMode Mode { get; private set; }

    public CaptureUtils(string server, IImageUtils imageUtils)
    {
        _server = server;
        _imageUtils = imageUtils;
    }

    #region Capture Mode
    public void StandByMode()
    {
        if (Mode != CaptureMode.StandBy)
        {
            Mode = CaptureMode.StandBy;
            Console.WriteLine($"{Mode}");
        }
    }
    public void LocationMode(CaptureProfile captureProfile, string origin, string destination)
    {
        if (Mode == CaptureMode.Location)
        {
            StandByMode();
        }
        else
        {
            _captureProfile = captureProfile;
            _origin = origin;
            _destination = destination;
        
            _location = new List<Tuple<string, int>>();
            Mode = CaptureMode.Location;
            Console.WriteLine($"{Mode}");
        }
    }
    public void QuantumMode()
    {
        if (Mode != CaptureMode.Quantum)
        {
            _location = new List<Tuple<string, int>>();
            _signature = new List<Tuple<string, int>>();
            Mode = CaptureMode.Quantum;
            Console.WriteLine($"{Mode}");
        }
    }
    public void Scanning(CaptureProfile captureProfile)
    {
        _captureProfile = captureProfile;
        if (Mode != CaptureMode.Scanning)
        {
            _signature = new List<Tuple<string, int>>();
            Mode = CaptureMode.Scanning;
            Console.WriteLine($"{Mode}");
        }
    }
    #endregion

    #region Capture Methods
    public bool CaptureSignature()
    {
        if (Mode == CaptureMode.Scanning)
        {
            // Get image and process it
            var captureBitmap = _imageUtils.GetScreenshot(_captureProfile);
            var processed = _imageUtils.ProcessImage(captureBitmap, _captureProfile);

            // Analyze image as memory stream
            MemoryStream stream = new MemoryStream();
            processed.Save(stream, ImageFormat.Png);
            GetSignature(stream);
        }

        // Set Mode to StandBy if required confidence level is reached
        // Mode = (GetSignature().Confidence >= _captureProfile.MinConfidence)
        //     ? CaptureMode.StandBy
        //     : CaptureMode.Scanning;

        // Signal caller whether to keep calling or not
        return Mode == CaptureMode.Scanning;
    }
    public bool CaptureLocation()
    {
        var location = GetLocationResult();
        if (location.Confidence <= _captureProfile.MinConfidence)
        {
            var captureBitmap = _imageUtils.GetScreenshot(_captureProfile);

            MemoryStream stream = new MemoryStream();
            captureBitmap.Save(stream, ImageFormat.Png);


            var dist = GetDistance(stream);
            if (dist == null)
            {
                Thread.Sleep(1000);
                return true;
            }
            else
            {
                _location.Add(dist);
            }
        }
        else
        {
            Mode = CaptureMode.StandBy;
            Console.WriteLine($"LOC: {location.Label} / DIST: {location.Value}");
        }

        // Signal caller whether to keep calling or not
        return Mode == CaptureMode.Location;
    }
    #endregion

    #region Result Methods
    public ConfidenceResult GetLocationResult()
    {
        var result = new ListUtils(_location).GetConfidence(_captureProfile.MinSamples, _captureProfile.MinConfidence);
        return result;
    }
    public ConfidenceResult GetSignatureResult()
    {
        var result = new ListUtils(_signature).GetConfidence(_captureProfile.MinSamples, _captureProfile.MinConfidence);
        return result;
    }
    private Tuple<RsSignature, int>? GetRoidArcheType()
    {
        var sig = GetSignatureResult().Value;
        foreach (RsSignature val in Enum.GetValues(typeof(RsSignature)))
        {
            if (sig % (int)val == 0)
            {
                return new Tuple<RsSignature, int>(val, sig / (int)val);
            }
        }
        return null;
    }
    #endregion

    #region Ocr Methods
    private Tuple<string, int>? GetDistance(MemoryStream stream)
    {
#if DEBUG
#pragma warning disable CS0168 // Variable is declared but never used
        try
        {
            Image.FromStream(stream).Save("Location.png", ImageFormat.Png);
        }
        catch (Exception pokemon)
        {
            // Gotta catch'em all!
        }
#pragma warning restore CS0168 // Variable is declared but never used
#endif

        // Setup OCR engine
        using var engine = new Tesseract.TesseractEngine(_captureProfile.TessData, "eng", Tesseract.EngineMode.Default);
        engine.DefaultPageSegMode = Tesseract.PageSegMode.SingleLine;
        engine.SetVariable("debug_file", "log.txt");

        // Process image
        using var img = Tesseract.Pix.LoadFromMemory(stream.ToArray());
        using var page = engine.Process(img);

        // Analyze results
        var text = page.GetText();
        if (text.Contains(_destination!))
        {
            var val = text.Split(_destination)[1].Trim();
            if (int.TryParse(val, out int dist))
            {
                return new Tuple<string, int>(_destination!, dist);
            }
        }
        return null;
    }
    private void GetSignature(MemoryStream stream)
    {
#if DEBUG
#pragma warning disable CS0168 // Variable is declared but never used
        try
        {
            Image.FromStream(stream).Save("Signature.png", ImageFormat.Png);
        }
        catch (Exception pokemon)
        {
            // Gotta catch'em all!
        }
#pragma warning restore CS0168 // Variable is declared but never used
#endif

        using (var engine = new Tesseract.TesseractEngine(_captureProfile.TessData, "eng", Tesseract.EngineMode.Default))
        {
            engine.DefaultPageSegMode = Tesseract.PageSegMode.SingleBlock;
            engine.SetVariable("debug_file", "log.txt");
            using (var img = Tesseract.Pix.LoadFromMemory(stream.ToArray()))
            {
                using (var page = engine.Process(img))
                {
                    var text = page.GetText();
                    foreach (int sig in Enum.GetValues(typeof(RsSignature)))
                    {
                        for (int i = 10; i > 0; i--)
                        {
                            if (text.Contains($"{sig * i}"))
                            {
                                Console.Write('.');
                                _signature.Add(new Tuple<string, int>(sig.ToString(), sig * i));
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Persistence Methods
    public void SaveHaloResults(string fullPath)
    {
        var dist = GetLocationResult();
        var sig = GetSignatureResult();
        var archetype = GetRoidArcheType();

        if (archetype!.Item2 == 0) {
            return;
        }

        if (sig.Confidence > _captureProfile.MinConfidence)
        {
            Console.WriteLine($"LOC: {dist.Label} / DIST: {dist.Value} ({dist.Confidence}) / SIG: {sig.Label} ({archetype?.Item1} * {archetype?.Item2}) / CONF: {sig.Confidence}");
        }

        var data = new List<ScanResult>();

        if (File.Exists(fullPath))
        {
            data = JsonConvert.DeserializeObject<List<ScanResult>>(File.ReadAllText(fullPath));
        }

        data!.Add(new ScanResult
        {
            Server = _server,
            Origin = _origin!,
            Destination = dist.Label,
            Distance = dist.Value,
            Signature = (int)archetype!.Item1,
            Quantity = archetype!.Item2,
            Confidence = sig.Confidence
        });

        var jsonData = JsonConvert.SerializeObject(data);
        File.WriteAllText(fullPath, jsonData);

        _signature = new List<Tuple<string, int>>();

        StandByMode();
    }

    internal void SetRoute(string server, string origin, string destination)
    {
        _server = server;
        _origin = origin;
        _destination = destination;
    }
    #endregion
}