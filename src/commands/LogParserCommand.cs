using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace CStone.Commands;

public class LogParserCommand : Command<LogParserCommand.Settings>
{
    private Settings _settings;
    private List<string> _lines = new List<string>();

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("Using settings from command line...");
        _settings = settings;

        ExtractLines();
        ParseLines();
        OutputLines();

        return 0;
    }

    private void OutputLines()
    {

        switch (_settings.Output)
        {
            case "console":

                foreach (var line in _lines)
                {
                    AnsiConsole.WriteLine(line);
                }
                break;
            case "uif":
                //before your loop
                var uif = new StringBuilder();

                foreach (var line in _lines)
                {
                    uif.AppendLine(line);
                }

                //after your loop
                File.WriteAllText(Path.Join(Environment.CurrentDirectory, "output.uif"), uif.ToString());
                break;
            case "csv":
                //before your loop
                var csv = new StringBuilder();
                csv.AppendLine("date,playerId,shopId,shopName,kioskId,kioskState,result,type,client_price,itemClassGUID,itemName,quantity");

                foreach (var line in _lines)
                {
                    csv.AppendLine(line);
                }

                //after your loop
                File.WriteAllText(Path.Join(Environment.CurrentDirectory, "output.csv"), csv.ToString());
                break;

        }

    }

    private void ParseLines()
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            var date = ExtractRegex<DateTime>(_lines[i], "date", new Regex("(?<date>[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{3}Z)"));
            var playerId = ExtractRegex<Int128>(_lines[i], "playerId", new Regex("playerId\\[(?<playerId>[0-9]+)\\]"));
            var shopId = ExtractRegex<Int128>(_lines[i], "shopId", new Regex("shopId\\[(?<shopId>[0-9]+)\\]"));
            var shopName = ExtractRegex<string>(_lines[i], "shopName", new Regex("shopName\\[(?<shopName>[a-zA-Z0-9-_]+)\\]"));
            var kioskId = ExtractRegex<Int128>(_lines[i], "kioskId", new Regex("kioskId\\[(?<kioskId>[0-9]+)\\]"));
            var kioskState = ExtractRegex<string>(_lines[i], "kioskState", new Regex("kioskState\\[(?<kioskState>[a-zA-Z]+)\\]"));
            var result = ExtractRegex<string>(_lines[i], "result", new Regex("result\\[(?<result>[a-zA-Z]+)\\]"));
            var type = ExtractRegex<string>(_lines[i], "type", new Regex("type\\[(?<type>[a-zA-Z]+)\\]"));

            var client_price = ExtractRegex<int>(_lines[i], "client_price", new Regex("client_price\\[(?<client_price>[0-9]+)"));
            var itemClassGUID = ExtractRegex<string>(_lines[i], "itemClassGUID", new Regex("itemClassGUID\\[(?<itemClassGUID>[0-9a-z-]+)\\]"));
            var itemName = ExtractRegex<string>(_lines[i], "itemName", new Regex("itemName\\[(?<itemName>[a-zA-Z0-9_]+)\\]"));
            var quantity = ExtractRegex<int>(_lines[i], "quantity", new Regex("quantity\\[(?<quantity>[0-9]+)\\]"));

            switch (_settings.Output)
            {
                case "uif":
                    _lines[i] = string.Format("{0},,,{1},,,,{2},,,{3}", itemClassGUID, (client_price / quantity).ToString(), date.AddYears(930).ToString("yyyy-MM-dd"), shopId.ToString());
                    break;
                default:
                    _lines[i] = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", date.ToString("yyyy-MM-dd HH:mm:ss"), playerId.ToString(), shopId.ToString(), shopName, kioskId.ToString(), kioskState, result, type, client_price, itemClassGUID, itemName, quantity);
                    break;
            }


        }
    }

    private T ExtractRegex<T>(string line, string group, Regex regex)
    {
        var val = regex.Match(line).Groups[group].Value;

        TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));
        object propValue = typeConverter.ConvertFromString(val);

        return (T)propValue;
    }

    private void ExtractLines()
    {
        foreach (string file in Directory.EnumerateFiles(_settings.Directory, "*.log"))
        {
            ExtractFile(file);
        }

        if (_settings.Output == "uif") {
            var gameLog = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roberts Space Industries/StarCitizen/LIVE/Game.log");
            ExtractFile(gameLog);
        }
    }

    private void ExtractFile(string file)
    {
        const int BufferSize = 128;
        using (var fileStream = File.OpenRead(file))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                if (line.Contains("CEntityComponentShopUIProvider::SendShopSellRequest"))
                {
                    if (_settings.Output == "raw")
                    {
                        AnsiConsole.WriteLine(line);
                    }
                    else
                    {
                        _lines.Add(line);
                    }
                }
            }
        }
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-d|--directory")]
        public required string Directory { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roberts Space Industries/StarCitizen/LIVE/logbackups");
        [CommandOption("-o|--output")]
        public required string Output { get; set; } = "console";
    }
}