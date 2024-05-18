using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace CStone.Commands;

public class LogParserCommand : Command<LogParserCommand.Settings>
{
    private Settings _settings;
    private ObservableCollection<string> _lines;
    private ObservableCollection<string> _parsed;
    private string _gameDirectory;
    private string _gameLogFileName;
    private string _gameLogFullPath;

    public LogParserCommand()
    {
        _parsed = new ObservableCollection<string>();
        _parsed.CollectionChanged += OnParsed;

        _lines = new ObservableCollection<string>();
        _lines.CollectionChanged += OnExtacted;
    }

    private void OnExtacted(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            var list = new List<string>();
            foreach (var item in e.NewItems)
            {
                list.Add(item.ToString());
            }
            ParseLines(list);
        }
    }

    private void OnParsed(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            OutputLines(e.NewItems);
        }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("Using settings from command line...");
        _settings = settings;

        ExtractLines();

        if (_settings.Watch)
        {
            Watch();
        }

        return 0;
    }

    private void OutputLines(System.Collections.IList? newItems)
    {

        switch (_settings.Output)
        {
            case "console":

                foreach (var line in newItems)
                {
                    AnsiConsole.WriteLine(line.ToString());
                }
                break;
            case "uif":
                //before your loop
                var uif = new StringBuilder();

                foreach (var line in newItems)
                {
                    uif.AppendLine(line.ToString());
                }

                //after your loop
                File.WriteAllText(Path.Join(Environment.CurrentDirectory, "output.uif"), uif.ToString());
                break;
            case "csv":
                //before your loop
                var csv = new StringBuilder();
                csv.AppendLine("date,playerId,shopId,shopName,kioskId,kioskState,result,type,client_price,itemClassGUID,itemName,quantity");

                foreach (var line in newItems)
                {
                    csv.AppendLine(line.ToString());
                }

                //after your loop
                File.WriteAllText(Path.Join(Environment.CurrentDirectory, "output.csv"), csv.ToString());
                break;

        }

    }

    private void ParseLines(IList<string> newItems)
    {
        for (int i = 0; i < newItems.Count; i++)
        {
            var date = ExtractRegex<DateTime>(newItems[i], "date", new Regex("(?<date>[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{3}Z)"));
            var playerId = ExtractRegex<Int128>(newItems[i], "playerId", new Regex("playerId\\[(?<playerId>[0-9]+)\\]"));
            var shopId = ExtractRegex<Int128>(newItems[i], "shopId", new Regex("shopId\\[(?<shopId>[0-9]+)\\]"));
            var shopName = ExtractRegex<string>(newItems[i], "shopName", new Regex("shopName\\[(?<shopName>[a-zA-Z0-9-_]+)\\]"));
            var kioskId = ExtractRegex<Int128>(newItems[i], "kioskId", new Regex("kioskId\\[(?<kioskId>[0-9]+)\\]"));
            var kioskState = ExtractRegex<string>(newItems[i], "kioskState", new Regex("kioskState\\[(?<kioskState>[a-zA-Z]+)\\]"));
            var result = ExtractRegex<string>(newItems[i], "result", new Regex("result\\[(?<result>[a-zA-Z]+)\\]"));
            var type = ExtractRegex<string>(newItems[i], "type", new Regex("type\\[(?<type>[a-zA-Z]+)\\]"));

            var client_price = ExtractRegex<int>(newItems[i], "client_price", new Regex("client_price\\[(?<client_price>[0-9]+)"));
            var itemClassGUID = ExtractRegex<string>(newItems[i], "itemClassGUID", new Regex("itemClassGUID\\[(?<itemClassGUID>[0-9a-z-]+)\\]"));
            var itemName = ExtractRegex<string>(newItems[i], "itemName", new Regex("itemName\\[(?<itemName>[a-zA-Z0-9_]+)\\]"));
            var quantity = ExtractRegex<int>(newItems[i], "quantity", new Regex("quantity\\[(?<quantity>[0-9]+)\\]"));

            switch (_settings.Output)
            {
                case "uif":
                    _parsed.Add(string.Format("{0},,,{1},,,,{2},,,{3}", itemClassGUID, (client_price / quantity).ToString(), date.AddYears(930).ToString("yyyy-MM-dd"), shopId.ToString()));
                    break;
                default:
                    _parsed.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", date.ToString("yyyy-MM-dd HH:mm:ss"), playerId.ToString(), shopId.ToString(), shopName, kioskId.ToString(), kioskState, result, type, client_price, itemClassGUID, itemName, quantity));
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
        if (IsDirectory(_settings.Path))
        {
            foreach (string file in Directory.EnumerateFiles(_settings.Path, "*.log"))
            {
                ExtractFile(file);
            }
        }
        else
        {
            ExtractFile(_settings.Path);
        }

        if (_settings.Output == "uif")
        {
            var gameLog = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roberts Space Industries/StarCitizen/LIVE/Game.log");
            ExtractFile(gameLog);
        }
    }

    private bool IsDirectory(string path)
    {
        FileAttributes attr = File.GetAttributes(path);
        return attr.HasFlag(FileAttributes.Directory);
    }

    private void ExtractFile(string file)
    {
        const int BufferSize = 128;
        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                if (line.Contains("CEntityComponentShopUIProvider::SendShopBuyRequest")
                    || line.Contains("CEntityComponentShoppingProvider::SendStandardItemBuyRequest")
                    || line.Contains("CEntityComponentShoppingProvider::SendRentalRequest")
                    )
                {
                    if (_settings.Output == "raw")
                    {
                        AnsiConsole.WriteLine(line);
                    }
                    else
                    {
                        if (_lines.IndexOf(line) == -1)
                        {
                            _lines.Add(line);
                        }
                    }
                }
            }
        }
    }

    private void Watch()
    {
        _gameDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roberts Space Industries\\StarCitizen\\LIVE\\");
        _gameLogFileName = "Game.log";
        _gameLogFullPath = Path.Join(_gameDirectory, _gameLogFileName);
        // Create a new FileSystemWatcher and set its properties.
        FileSystemWatcher watcher = new FileSystemWatcher
        {
            Path = _gameDirectory,
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            NotifyFilter = NotifyFilters.Size,
            // Only watch text files.
            Filter = _gameLogFileName
        };

        // Add event handlers.
        watcher.Changed += new FileSystemEventHandler(OnChanged);
        watcher.Changed += new FileSystemEventHandler(OnChanged);

        // Begin watching.
        watcher.EnableRaisingEvents = true;

        new AutoResetEvent(false).WaitOne();
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
        ExtractFile(e.FullPath);
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-p|--path")]
        public required string Path { get; set; } = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roberts Space Industries/StarCitizen/LIVE/logbackups");
        [CommandOption("-o|--output")]
        public required string Output { get; set; } = "console";
        [CommandOption("-w|--watch")]
        public required bool Watch { get; set; }
    }
}