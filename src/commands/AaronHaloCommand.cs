#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using CStone.Types;
using SharpHook;
using SharpHook.Native;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CStone.Commands;

public class AaronHaloCommand : Command<AaronHaloCommand.Settings>
{
    private Settings _settings;
    private CaptureUtils _capture;
    private string _resultsFile;
#pragma warning disable IDE0090 // Use 'new(...)'
    private static CaptureProfile _locationProfile = new CaptureProfile
#pragma warning restore IDE0090 // Use 'new(...)'
    {
        Width = 400,
        Height = 42,
        X = 2450,
        Y = 80,
        MinSamples = 5,
        MinConfidence = 0.7f,
        ShipModel = ShipModel.Vulture,
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX")!
    };
#pragma warning disable IDE0090 // Use 'new(...)'
    private static CaptureProfile _signatureProfile = new CaptureProfile
#pragma warning restore IDE0090 // Use 'new(...)'
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
        TessData = Environment.GetEnvironmentVariable("TESSDATA_PREFIX")!
    };

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("Using settings from command line...");
        _settings = settings;

        var statusPrefix = $"HALO ({_settings.Server}): {_settings.Origin.Replace(' ', '-')} <> {_settings.Destination.Replace(' ', '-')}";

        var status = AnsiConsole.Status().AutoRefresh(false);
            status.Start(statusPrefix, ctx => {

                AnsiConsole.MarkupLine("Initializing capture utils...");
                _capture = new CaptureUtils(_settings.Server, new ImageUtils(), (loc) => {
                        if (loc == null) {
                            ctx.Status = $"{statusPrefix} : {_capture.Mode}";
                            ctx.Refresh();
                            return;
                        }
                        if (loc.Value.Count >= _locationProfile.MinSamples && loc.Value.Confidence >= _locationProfile.MinConfidence) {
                            Console.Beep(37, 1);
                            ctx.Status = $"{statusPrefix.Replace("<>", $"<--{loc.Value.Value}-->")} MODE: {_capture.Mode}";
                            ctx.Refresh();
                        }
                }, (sig) => {
                    if (sig != null && sig.Value.Count > 2) {

                        if (sig.Value.Count >= _signatureProfile.MinSamples && sig.Value.Confidence >= _signatureProfile.MinConfidence) {
                            Console.Beep(37, 1);
                        }
                        
                        ctx.Status = $"{new string('-', sig.Value.Count)} / {sig.Value.Label} / {sig.Value.Confidence}";
                        ctx.Refresh();
                    }
                }, (result) => {
                    ctx.Status = $"{result.Server} {result.Origin.Replace(' ', '-')} {result.Destination.Replace(' ', '-')} {result.Distance} {result.Archetype} * {result.Quantity} = {result.Signature * result.Quantity} ({result.Confidence})";
                    ctx.Refresh();
                });

                AnsiConsole.MarkupLine("Setting results file...");
                _resultsFile = $"{_settings.Origin.Replace(' ', '-')}_{_settings.Destination.Replace(' ', '-')}.json";

                using (var hook = new TaskPoolGlobalHook())
                {
                    AnsiConsole.MarkupLine("Binding keys...");
                    hook.KeyReleased += OnKeyReleased;
                    hook.KeyPressed += OnKeyPressed;
                    hook.RunAsync();
                    AnsiConsole.MarkupLine("Keyboard hooks running...");

                    while (ShouldContinue)
                    {
                        Scan(ctx);
                    }
                }
            });

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

    private bool ShouldContinue {get; set;} = true;

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcF2:
                if (_capture.GetLocationResult().Confidence >= _locationProfile.MinConfidence)
                {
                    _capture.StandByMode();
                }
                else
                {
                    _capture.LocationMode(_locationProfile, _settings.Origin, _settings.Destination);
                }
                break;
            case KeyCode.VcV:
                _capture.SaveHaloResults(_resultsFile);
                break;
            case KeyCode.VcB:
                _capture.QuantumMode();
                break;
        }
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcV:
                _capture.Scanning(_signatureProfile);
                break;
        }
    }

    private void Scan(StatusContext ctx)
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
                    //TODO: Use AnsiConsole.Progress)
                    // AnsiConsole.Markup(".");
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