#pragma warning disable CA1416 // Validate platform compatibility

using SharpHook;
using SharpHook.Native;

namespace SCCapture;

public enum CaptureMode
{
    StandBy = 0,
    Scanning = 1,
    Quantum = 2,
    Location = 3
}

public enum RsSignature
{
    Junk = 1000,
    C = 1700,
    E = 1900,
    M = 1850,
    P = 1750,
    Q = 1870,
    S = 1720,
}

class Program
{
    //public static CaptureMode utils.Mode = CaptureMode.StandBy;
    private static CaptureProfile _locationProfile = new CaptureProfile
    {
        Width = 400,
        Height = 42,
        X = 2450,
        Y = 80,
        MinSamples = 5,
        MinConfidence = 0.7f,
        ShipModel = ShipModel.Vulture,
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX") ?? "C:/Users/JaniHyytiäinen/src/Cstone.Capture/tessdata"
    };
    private static CaptureProfile _signatureProfile = new CaptureProfile
    {
        Width = 160,
        Height = 160,
        X = 1010,    // Mole
        // X = 935,     // Vanguard
        // X = 1060,    // Vulture
        Y = 550,     // Mole
        // Y = 410,     // Vanguard     
        // Y = 490,     // Vulture
        MinSamples = 5,
        MinConfidence = 0.7f,        
        ShipModel = ShipModel.Vulture,
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX") ?? "C:/Users/JaniHyytiäinen/src/Cstone.Capture/tessdata"
    };
    private static CaptureUtils _capture = new CaptureUtils(_locationProfile, _signatureProfile, new ImageUtils());

    static void Main(string[] args)
    {
        _capture.SetServer(args[0]);
        _capture.SetOrigin(args[1]);
        _capture.SetDestination(args[2]);

        using (var hook = new TaskPoolGlobalHook())
        {
            hook.KeyReleased += OnKeyReleased;
            hook.KeyPressed += OnKeyPressed;
            hook.RunAsync();
            while (true)
            {
                Program.Scan();
            }
        }
    }

    private static void Scan()
    {
        switch (_capture.Mode)
        {
            case CaptureMode.StandBy:
                while (_capture.Mode == CaptureMode.StandBy)
                {
                    Thread.Sleep(1000);
                }
                break;
            case CaptureMode.Location:
                while (_capture.CaptureLocation())
                {
                    Console.Write('.');
                }
                break;
            case CaptureMode.Scanning:
                while (_capture.CaptureSignature())
                {
                    // Console.Write('.');
                }
                break;
        }
    }
    private static void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcF2:
                if (_capture.GetLocation().Confidence >= _locationProfile.MinConfidence) {
                    _capture.StandBy();
                } else {
                    _capture.Location();
                }
                break;
            case KeyCode.VcV:
                _capture.Save();
                break;
            case KeyCode.VcB:
                _capture.Quantum();
                break;
        }
    }
    private static void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcV:
                _capture.Scanning();
                break;
        }
    }
}
