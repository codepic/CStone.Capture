#pragma warning disable CA1416 // Validate platform compatibility

using CStone.Commands;
using Spectre.Console.Cli;

namespace CStone;

class Program
{
    static void Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
#if DEBUG
            config.PropagateExceptions();
            config.ValidateExamples();
#endif
            
            config.AddCommand<AaronHaloCommand>("halo")
                .WithDescription("Capture asteroid data from Aaron Halo.");
            config.AddCommand<LogParserCommand>("parse-logs")
                .WithDescription("Parse Game.log for further use.");
        });
        app.Run(args);
    }
}
