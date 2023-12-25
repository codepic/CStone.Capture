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
        // Mole         1010
        // Vanguard     935
        X = 1060,
        // Mole         550
        // Vanguard     410
        Y = 490,
        MinSamples = 5,
        MinConfidence = 0.7f,        
        ShipModel = ShipModel.Vulture,
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX") ?? "C:/Users/JaniHyytiäinen/src/Cstone.Capture/tessdata"
    };
    private static CaptureUtils _capture = new CaptureUtils(_locationProfile, _signatureProfile, new ImageUtils());

    static void Main(string[] args)
    {
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
                _capture.Location();
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
