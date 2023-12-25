using System.Drawing;
using System.Drawing.Imaging;

namespace SCCapture;
public class CaptureUtils
{
    private readonly CaptureProfile _locationProfile;
    private readonly CaptureProfile _signatureProfile;
    private IList<Tuple<string, int>> _location = new List<Tuple<string, int>>();
    private IList<Tuple<string, int>> _signature = new List<Tuple<string, int>>();
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
        Mode = CaptureMode.StandBy;
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
        }
    }
    public void Quantum()
    {
        if (Mode != CaptureMode.Quantum)
        {
            _location = new List<Tuple<string, int>>();
            _signature = new List<Tuple<string, int>>();
            Mode = CaptureMode.Quantum;
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
        // Console.WriteLine($"CaptureSignature {mode}");
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

        return Mode == CaptureMode.Scanning;
    }
    public bool CaptureLocation()
    {
        _location = new List<Tuple<string, int>>();
        var width = _locationProfile.Width;
        var height = _locationProfile.Height;
        var x = _locationProfile.X; // Mole 2450
        var y = _locationProfile.Y; // Mole 80

        var captureBitmap = new Bitmap(width, height);
        var graphics = Graphics.FromImage(captureBitmap);
        var upperLeftSource = new Point(x, y);
        var upperLeftDestination = new Point(0, 0);
        graphics.CopyFromScreen(upperLeftSource, upperLeftDestination, captureBitmap.Size);
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

        return GetLocation().Confidence > _locationProfile.MinConfidence;
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
    private static Tuple<string, int>? GetDistance(MemoryStream stream)
    {
        // Setup OCR engine
        using var engine = new Tesseract.TesseractEngine("C:/Users/JaniHyytiäinen/src/SCCapture/tessdata", "eng", Tesseract.EngineMode.Default);
        engine.DefaultPageSegMode = Tesseract.PageSegMode.SingleLine;
        engine.SetVariable("debug_file", "log.txt");

        // Process image
        using var img = Tesseract.Pix.LoadFromMemory(stream.ToArray());
        using var page = engine.Process(img);

        // Analyze results
        var text = page.GetText();
        var parts = text.Split(' ');
        if (parts.Length == 2)
        {
            var name = parts[0].Trim();
            var val = parts[1].Trim();
            if (name == "Stanton")
            {
                if (int.TryParse(val, out int dist))
                {
                    return new Tuple<string, int>(name, dist);
                }
            }
        }
        return null;
    }
    private void GetSignature(MemoryStream stream)
    {
        // try
        // {
        //     Image.FromStream(stream).Save("Signature.png", ImageFormat.Png);
        // }
        // catch (Exception pokemon)
        // {
        //     // Gotta catch'em all!
        // }

        using (var engine = new Tesseract.TesseractEngine("C:/Users/JaniHyytiäinen/src/SCCapture/src/tessdata", "eng", Tesseract.EngineMode.Default))
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
                                Console.WriteLine($"{_signature.Count} : {sig} / {sig * i}");
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

        Console.WriteLine(sig.Confidence);

        if (sig.Confidence > _signatureProfile.MinConfidence)
        {
            Console.WriteLine($"LOC: {dist.Label} / DIST: {dist.Value} ({dist.Confidence}) / SIG: {sig.Label} ({archetype?.Item1} * {archetype?.Item2}) / CONF: {sig.Confidence}");
        }

        StandBy();
    }
    #endregion
}