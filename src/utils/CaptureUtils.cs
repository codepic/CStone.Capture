using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;

namespace SCCapture;
public class CaptureUtils
{
    private readonly CaptureProfile _locationProfile;
    private readonly CaptureProfile _signatureProfile;
    private IList<Tuple<string, int>> _location = new List<Tuple<string, int>>();
    private IList<Tuple<string, int>> _signature = new List<Tuple<string, int>>();
    private string _server;
    private string _origin;
    private string _destination;
    private readonly IImageUtils _imageUtils;

    public CaptureMode Mode { get; private set; }

    public CaptureUtils(CaptureProfile locationProfile, CaptureProfile signatureProfile, IImageUtils imageUtils)
    {
        _locationProfile = locationProfile;
        _signatureProfile = signatureProfile;
        _imageUtils = imageUtils;
    }

    #region Capture Mode
    public void StandBy()
    {
        if (Mode != CaptureMode.StandBy)
        {
            Mode = CaptureMode.StandBy;
            Console.WriteLine($"{Mode}");
        }
    }
    public void Location()
    {
        if (Mode == CaptureMode.Location)
        {
            StandBy();
        }
        else
        {
            _location = new List<Tuple<string, int>>();
            Mode = CaptureMode.Location;
            Console.WriteLine($"{Mode}");
        }
    }
    public void Quantum()
    {
        if (Mode != CaptureMode.Quantum)
        {
            _location = new List<Tuple<string, int>>();
            _signature = new List<Tuple<string, int>>();
            Mode = CaptureMode.Quantum;
            Console.WriteLine($"{Mode}");
        }
    }
    public void Scanning()
    {
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
            var captureBitmap = _imageUtils.GetScreenshot(_signatureProfile);
            var processed = _imageUtils.ProcessImage(captureBitmap, _signatureProfile);

            // Analyze image as memory stream
            MemoryStream stream = new MemoryStream();
            processed.Save(stream, ImageFormat.Png);
            GetSignature(stream);
        }

        // Set Mode to StandBy if required confidence level is reached
        // Mode = (GetSignature().Confidence >= _signatureProfile.MinConfidence)
        //     ? CaptureMode.StandBy
        //     : CaptureMode.Scanning;

        // Signal caller whether to keep calling or not
        return Mode == CaptureMode.Scanning;
    }
    public bool CaptureLocation()
    {
        var location = GetLocation();
        if (location.Confidence <= _locationProfile.MinConfidence)
        {
            var captureBitmap = _imageUtils.GetScreenshot(_locationProfile);

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
    public ConfidenceResult GetLocation()
    {
        var result = new ListUtils(_location).GetConfidence(_locationProfile.MinSamples, _locationProfile.MinConfidence);
        return result;
    }
    public ConfidenceResult GetSignature()
    {
        var result = new ListUtils(_signature).GetConfidence(_signatureProfile.MinSamples, _signatureProfile.MinConfidence);
        return result;
    }
    public Tuple<RsSignature, int>? GetArcheType()
    {
        var sig = GetSignature().Value;
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
        try
        {
            Image.FromStream(stream).Save("Location.png", ImageFormat.Png);
        }
        catch (Exception pokemon)
        {
            // Gotta catch'em all!
        }

        // Setup OCR engine
        using var engine = new Tesseract.TesseractEngine(_locationProfile.TessData, "eng", Tesseract.EngineMode.Default);
        engine.DefaultPageSegMode = Tesseract.PageSegMode.SingleLine;
        engine.SetVariable("debug_file", "log.txt");

        // Process image
        using var img = Tesseract.Pix.LoadFromMemory(stream.ToArray());
        using var page = engine.Process(img);

        // Analyze results
        var text = page.GetText();
        if (text.Contains(_destination))
        {
            var val = text.Split(_destination)[1].Trim();
            if (int.TryParse(val, out int dist))
            {
                return new Tuple<string, int>(_destination, dist);
            }
        }
        return null;
    }
    private void GetSignature(MemoryStream stream)
    {
        try
        {
            Image.FromStream(stream).Save("Signature.png", ImageFormat.Png);
        }
        catch (Exception pokemon)
        {
            // Gotta catch'em all!
        }

        using (var engine = new Tesseract.TesseractEngine(_signatureProfile.TessData, "eng", Tesseract.EngineMode.Default))
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
    public void Save()
    {
        var dist = GetLocation();
        var sig = GetSignature();
        var archetype = GetArcheType();

        if (archetype!.Item2 == 0) {
            return;
        }

        if (sig.Confidence > _signatureProfile.MinConfidence)
        {
            Console.WriteLine($"LOC: {dist.Label} / DIST: {dist.Value} ({dist.Confidence}) / SIG: {sig.Label} ({archetype?.Item1} * {archetype?.Item2}) / CONF: {sig.Confidence}");
        }

        var data = new List<ScanResult>();

        var fileName = $"{_origin.Replace(' ', '-')}_{_destination.Replace(' ', '-')}.json";
        if (File.Exists(fileName))
        {
            data = JsonConvert.DeserializeObject<List<ScanResult>>(File.ReadAllText(fileName));
        }

        data!.Add(new ScanResult
        {
            Server = _server,
            Origin = _origin,
            Destination = dist.Label,
            Distance = dist.Value,
            Signature = (int)archetype!.Item1,
            Quantity = archetype!.Item2,
            Confidence = sig.Confidence
        });

        var jsonData = JsonConvert.SerializeObject(data);
        File.WriteAllText(fileName, jsonData);

        _signature = new List<Tuple<string, int>>();

        StandBy();
    }

    internal void SetServer(string server)
    {
        _server = server;
    }

    internal void SetOrigin(string origin)
    {
        _origin = origin;
    }

    internal void SetDestination(string destination)
    {
        _destination = destination;
    }
    #endregion
}