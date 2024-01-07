#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using SharpHook;
using SharpHook.Native;
using Spectre.Console.Cli;

namespace CStone.Commands;

public class AaronHaloCommand : Command<AaronHaloCommand.Settings>
{
    private Settings _settings;
    private static CaptureUtils _capture;

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

    public override int Execute(CommandContext context, Settings settings)
    {
        _settings = settings;

        Console.WriteLine($"{_settings.Server} {_settings.Origin} {_settings.Destination}");

        new CaptureUtils(_locationProfile, _signatureProfile, new ImageUtils());

        using (var hook = new TaskPoolGlobalHook())
        {
            hook.KeyReleased += OnKeyReleased;
            hook.KeyPressed += OnKeyPressed;
            hook.RunAsync();
            while (true)
            {
                Scan();
            }
        }

        return 0;
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-s|--server")]
        public string Server { get; set; }

        [CommandOption("-o|--origin")]
        public string Origin { get; set; }

        [CommandOption("-d|--dest|--destination")]
        public string Destination { get; set; }
    }

    private static void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcF2:
                if (_capture.GetLocation().Confidence >= _locationProfile.MinConfidence)
                {
                    _capture.StandBy();
                }
                else
                {
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

}