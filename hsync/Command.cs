// This source code is a part of project violet-server.
// Copyright (C)2020-2021. violet-team. Licensed under the MIT Licence.

using Extreme.Mathematics;
using Extreme.Statistics;
using hsync.CL;
using hsync.Component;
using hsync.Log;
using hsync.Network;
using hsync.Setting;
using hsync.Utils;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace hsync
{
    public class Options : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION)]
        public bool Help;
        [CommandLine("--version", CommandType.OPTION, ShortOption = "-v", Info = "Show version information.")]
        public bool Version;

        /// <summary>
        /// Atomic Options
        /// </summary>

        [CommandLine("--recover-settings", CommandType.OPTION, Info = "Recover settings.json")]
        public bool RecoverSettings;

        [CommandLine("--related-tag-test", CommandType.ARGUMENTS, ArgumentsCount = 2, ShortOption = "-r",
            Info = "Related Tag Test", Help = "use --related-tag-test <db file path> <threshold>")]
        public string[] RelatedTagTest;

        [CommandLine("--character-test", CommandType.ARGUMENTS, ArgumentsCount = 2, ShortOption = "-h",
            Info = "Character Test", Help = "use --character-tag-test <db file path> <threshold>")]
        public string[] CharacterTest;

        [CommandLine("--series-test", CommandType.ARGUMENTS, ArgumentsCount = 2, ShortOption = "-p",
            Info = "Series Test", Help = "use --series-tag-test <db file path> <threshold>")]
        public string[] SeriesTest;

        [CommandLine("--create-ehentai-inv-table", CommandType.OPTION,
            Info = "create e/exhentai hash inverse table", Help = "use --create-ehentai-inv-table")]
        public bool CreateEHentaiInverseTable;

        [CommandLine("--create-datetime-estimator", CommandType.OPTION,
            Info = "create datetime estimator", Help = "use --create-datetime-estimator")]
        public bool CreateDateTimeEstimator;

        [CommandLine("--init-server", CommandType.OPTION,
            Info = "Upload all data to server database", Help = "use --init-server")]
        public bool InitServer;

        /// <summary>
        /// User Option
        /// </summary>

        [CommandLine("--start", CommandType.OPTION, Default = true, ShortOption = "-s",
            Info = "Starts hsync", Help = "use --start")]
        public bool Start;

        [CommandLine("--compress", CommandType.OPTION, ShortOption = "-c",
            Info = "Compress exists data", Help = "use --compress")]
        public bool Compress;

        [CommandLine("--include-exhentai", CommandType.OPTION, ShortOption = "-x",
            Info = "Include ExHentai Database", Help = "use --include-exhentai")]
        public bool IncludeExHetaiData;

        [CommandLine("--low-perf", CommandType.OPTION, ShortOption = "-l",
            Info = "hsync run on low performance system", Help = "use --low-perf")]
        public bool LowPerf;

        [CommandLine("--sync-only", CommandType.OPTION, ShortOption = "-n",
            Info = "Sync only when start", Help = "use --sync-only")]
        public bool SyncOnly;

        [CommandLine("--hitomi-sync-range", CommandType.ARGUMENTS, ArgumentsCount = 2,
            Info = "Set lookup id range manually", Help = "use --hitomi-sync-range <start id> <end id>")]
        public string[] HitomiSyncRange;
        [CommandLine("--hitomi-sync-lookup-range", CommandType.ARGUMENTS,
            Info = "Set hitomi id lookup range. (default: 4,000 [-4,000 ~ 4,000])", Help = "use --hitomi-sync-lookup-range <count>")]
        public string[] HitomiSyncLookupRange;

        [CommandLine("--exhentai-lookup-page", CommandType.ARGUMENTS,
            Info = "Set exhentai lookup page. (default: 200)", Help = "use --exhentai-lookup-page <range>")]
        public string[] ExHentaiLookupPage;

        [CommandLine("--use-server", CommandType.OPTION, ShortOption = "-e",
            Info = "Upload sync data to server database", Help = "use --use-server")]
        public bool UseServer;
    }

    public class Command
    {
        public static void Start(string[] arguments)
        {
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            var option = CommandLineParser.Parse<Options>(arguments);

            //
            //  Single Commands
            //
            if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Version)
            {
                PrintVersion();
            }
            else if (option.RecoverSettings)
            {
                Settings.Instance.Recover();
                Settings.Instance.Save();
            }
            else if (option.RelatedTagTest != null)
            {
                ProcessRelatedTagTest(option.RelatedTagTest);
            }
            else if (option.CharacterTest != null)
            {
                ProcessCharacterTest(option.CharacterTest);
            }
            else if (option.SeriesTest != null)
            {
                ProcessSeriesTest(option.SeriesTest);
            }
            else if (option.Start)
            {
                ProcessStart(option.IncludeExHetaiData, option.LowPerf, option.SyncOnly, option.UseServer,
                    option.HitomiSyncRange, option.HitomiSyncLookupRange, option.ExHentaiLookupPage);
            }
            else if (option.Compress)
            {
                ProcessCompress(option.IncludeExHetaiData, option.LowPerf);
            }
            else if (option.CreateEHentaiInverseTable)
            {
                ProcessCreateEHentaiInverseTable(option.LowPerf);
            }
            else if (option.CreateDateTimeEstimator)
            {
                ProcessCreateDateTimeEstimator(option.LowPerf);
            }
            else if (option.InitServer)
            {
                ProcessInitServer(null);
            }
            else if (option.Error)
            {
                Console.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.WriteLine(option.HelpMessage);
                return;
            }
            else
            {
                Console.WriteLine("Nothing to work on.");
                Console.WriteLine("Enter './hsync --help' to get more information");
            }

            return;
        }

        static byte[] art_console = {
            0x8D, 0x54, 0x3D, 0x6F, 0xDB, 0x30, 0x10, 0xDD, 0x0D, 0xF8, 0x3F, 0x5C, 0xB8, 0x48, 0x01, 0x74, 0xE4, 0x18, 0xC0, 0xB0, 0xBC,
            0x0B, 0x59, 0xB3, 0x18, 0x50, 0x45, 0x23, 0xD9, 0x0A, 0x01, 0xDD, 0x3A, 0x34, 0x2E, 0x7F, 0x7B, 0xDF, 0xBB, 0x93, 0x14, 0xA7,
            0x92, 0xDB, 0xD0, 0x24, 0x41, 0x1F, 0xDF, 0xDD, 0xBD, 0xFB, 0xA0, 0x44, 0xBE, 0x38, 0xD2, 0x8B, 0xA4, 0x6E, 0xBF, 0xFB, 0x0F,
            0x48, 0xAE, 0x98, 0x12, 0xB5, 0xA4, 0x7F, 0x41, 0x5F, 0x7A, 0x39, 0x8B, 0x74, 0x42, 0x34, 0x74, 0x24, 0xDF, 0x80, 0xE1, 0xE7,
            0xF3, 0xB8, 0x4A, 0x4F, 0xA4, 0xE1, 0xCF, 0x9F, 0x2D, 0x77, 0x32, 0x52, 0xA3, 0xFB, 0x30, 0x0B, 0x18, 0x26, 0xA4, 0xD8, 0x61,
            0x68, 0xC6, 0xFA, 0x0D, 0xBD, 0x8E, 0x93, 0x07, 0xB7, 0x4A, 0xF5, 0x5E, 0x2E, 0x3C, 0x9C, 0x01, 0xCD, 0xD9, 0x2E, 0x4C, 0x8A,
            0x0D, 0x88, 0x11, 0xB2, 0x71, 0xC6, 0x09, 0x91, 0x39, 0xCA, 0x15, 0xD0, 0x5E, 0x8A, 0x42, 0x7A, 0x31, 0x29, 0x36, 0x4E, 0xC8,
            0xFC, 0x24, 0x97, 0xC8, 0x1C, 0xD0, 0x0D, 0x09, 0x50, 0x52, 0x34, 0x4A, 0xC0, 0xA2, 0x09, 0xFC, 0x1F, 0x62, 0xC6, 0x72, 0x49,
            0x72, 0x04, 0xA0, 0x51, 0x15, 0x38, 0x90, 0x28, 0xEA, 0xBE, 0x78, 0xCA, 0x51, 0x01, 0x0B, 0x02, 0x79, 0xC2, 0xE2, 0x89, 0x61,
            0x9D, 0x94, 0xBA, 0x34, 0x2B, 0xBC, 0x92, 0x72, 0xD2, 0xC0, 0x50, 0x03, 0x68, 0x88, 0x3C, 0x4D, 0xEB, 0xDB, 0x7E, 0x07, 0x57,
            0x39, 0x97, 0x00, 0x38, 0x50, 0xD4, 0x78, 0x17, 0x23, 0x47, 0x2E, 0xCC, 0x48, 0x24, 0x01, 0x67, 0x7A, 0x44, 0x02, 0x80, 0xA4,
            0xDD, 0xB9, 0x1A, 0x99, 0x97, 0xB4, 0xC8, 0x74, 0xA1, 0x68, 0x72, 0xF0, 0x98, 0x64, 0x80, 0x3D, 0x33, 0x38, 0x8D, 0x52, 0x2F,
            0xD0, 0x53, 0xCC, 0x07, 0x4B, 0xF1, 0x08, 0xCF, 0x18, 0x4B, 0xC1, 0x06, 0x90, 0x68, 0x20, 0x80, 0xFB, 0x30, 0xDB, 0x7E, 0x00,
            0x0D, 0x8D, 0xE4, 0x37, 0x22, 0x40, 0x37, 0x05, 0x0A, 0x7F, 0xB7, 0x0F, 0xAD, 0x93, 0x57, 0x4D, 0x52, 0x95, 0x89, 0x02, 0x95,
            0x1A, 0x72, 0xD2, 0xF6, 0x15, 0xA4, 0xF3, 0xE3, 0xAA, 0xE7, 0x26, 0x2D, 0x90, 0x22, 0xA1, 0xA5, 0xC5, 0xAC, 0x9E, 0x18, 0x6F,
            0xA1, 0xFC, 0x90, 0x7E, 0xDD, 0xA9, 0xBD, 0x13, 0x41, 0x15, 0x99, 0x5C, 0x13, 0xC5, 0x81, 0x72, 0x63, 0x5E, 0x2C, 0xF3, 0x6B,
            0x67, 0x8B, 0x3B, 0x96, 0xCE, 0x23, 0xF1, 0xDD, 0x82, 0xB4, 0xF1, 0xB8, 0xF9, 0x2C, 0x12, 0x7E, 0x68, 0x2B, 0x91, 0xCA, 0x8A,
            0x59, 0x4D, 0x5C, 0x67, 0x65, 0xA9, 0xB6, 0x94, 0x2C, 0xDF, 0x71, 0x86, 0x65, 0x73, 0x1A, 0xF5, 0x78, 0xFB, 0x94, 0x6E, 0x5D,
            0x18, 0xB8, 0x62, 0xE1, 0x83, 0xC5, 0x95, 0xAC, 0x63, 0x3F, 0x00, 0xCD, 0xAF, 0x76, 0x95, 0xF3, 0xD9, 0x91, 0xB9, 0x40, 0xCE,
            0xBD, 0xA8, 0xCF, 0xD8, 0x51, 0x20, 0x36, 0xAA, 0x8D, 0x74, 0xF7, 0xA9, 0x07, 0x6D, 0x2C, 0x79, 0xCC, 0x76, 0x07, 0x87, 0xD6,
            0x2E, 0x39, 0xBF, 0xAB, 0x2A, 0x5A, 0xA4, 0x6E, 0xEF, 0x79, 0x24, 0xDF, 0xC4, 0x42, 0x1B, 0xC3, 0xA3, 0x91, 0x40, 0xB1, 0xA7,
            0x8B, 0x7B, 0x3A, 0xE8, 0x8A, 0xE4, 0x01, 0x2D, 0x91, 0x35, 0x3F, 0x5B, 0x10, 0xA8, 0xEB, 0x6D, 0x95, 0x88, 0x07, 0x88, 0xD4,
            0x3B, 0x14, 0xD6, 0x7F, 0xA3, 0x9F, 0x53, 0x6A, 0xDB, 0x96, 0x8F, 0x6F, 0x53, 0x85, 0x85, 0xAA, 0x98, 0x09, 0xC4, 0x8F, 0x3E,
            0x16, 0x46, 0x82, 0x30, 0x74, 0x03, 0x8C, 0x7E, 0xF1, 0x2E, 0xB5, 0xB4, 0xE1, 0x8B, 0x63, 0xFC, 0xC7, 0x71, 0x0D, 0xB5, 0x2A,
            0xDA, 0xC4, 0xD3, 0x92, 0xC3, 0xDC, 0x2A, 0xF8, 0x9C, 0x6C, 0xB6, 0xB3, 0x15, 0xE3, 0x8A, 0xDF, 0x77, 0x7F, 0xEF, 0x3E, 0xCA,
            0xB0, 0xC1, 0xA1, 0xA0, 0x1D, 0xEA, 0x1C, 0x07, 0xD4, 0x7C, 0xBF, 0xFB, 0x03,
        };

        static void PrintHelp()
        {
            PrintVersion();
            Console.WriteLine(Encoding.UTF8.GetString(CompressUtils.Decompress(art_console)));
            Console.WriteLine($"Copyright (C) 2020-2021. project violet-server.");
            Console.WriteLine("Usage: ./hsync [OPTIONS...]");

            var builder = new StringBuilder();
            CommandLineParser.GetFields(typeof(Options)).ToList().ForEach(
                x =>
                {
                    var key = x.Key;
                    if (!key.StartsWith("--"))
                        return;
                    if (!string.IsNullOrEmpty(x.Value.Item2.ShortOption))
                        key = $"{x.Value.Item2.ShortOption}, " + key;
                    var help = "";
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        help = $"[{x.Value.Item2.Help}]";
                    if (!string.IsNullOrEmpty(x.Value.Item2.Info))
                        builder.Append($"   {key}".PadRight(30) + $" {x.Value.Item2.Info} {help}\r\n");
                    else
                        builder.Append($"   {key}".PadRight(30) + $" {help}\r\n");
                });
            Console.Write(builder.ToString());
        }

        public static void PrintVersion()
        {
            Console.WriteLine($"{Version.Name} {Version.Text}");
            Console.WriteLine($"Build Date: " + Internals.GetBuildDate().ToLongDateString());
        }

        static void ProcessStart(bool include_exhentai, bool low_perf, bool sync_only, bool use_server,
            string[] hitomi_sync_range, string[] hitomi_sync_lookup_range, string[] exhentai_lookup_page)
        {
            Console.Clear();
            Console.Title = "hsync";

            Console.WriteLine($"hsync - DB Synchronization Manager");
            Console.WriteLine($"Copyright (C) 2020. project violet-server.");
            Console.WriteLine($"Version: {Version.Text} (Build: {Internals.GetBuildDate().ToLongDateString()})");
            Console.WriteLine("");

            if (!low_perf)
            {
                if (!File.Exists("hiddendata.json"))
                {
                    Logs.Instance.Push("Welcome to hsync!\r\n\tDownload the necessary data before running the program!");
                    download_data("https://github.com/project-violet/database/releases/download/rd2020.06.07/hiddendata.json", "hiddendata.json");
                }
                if (!File.Exists("metadata.json"))
                    download_data("https://github.com/project-violet/database/releases/download/rd2020.06.07/metadata.json", "metadata.json");
                if (!File.Exists("ex-hentai-archive.json"))
                    download_data("https://github.com/project-violet/database/releases/download/rd2020.06.07/ex-hentai-archive.json", "ex-hentai-archive.json");

                var sync = new Syncronizer(hitomi_sync_range, hitomi_sync_lookup_range, exhentai_lookup_page);
                sync.SyncHitomi();
                sync.SyncExHentai();

                if (sync_only) return;

                var dbc = new DataBaseCreator();
                dbc.Integrate();
                dbc.ExtractRawDatabase("rawdata", include_exhentai: include_exhentai);
                Console.WriteLine("Complete all!");
                dbc.ExtractRawDatabase("rawdata-chinese", false, "chinese", include_exhentai: include_exhentai);
                Console.WriteLine("Complete chinese!");
                dbc.ExtractRawDatabase("rawdata-english", false, "english", include_exhentai: include_exhentai);
                Console.WriteLine("Complete english!");
                dbc.ExtractRawDatabase("rawdata-japanese", false, "japanese", include_exhentai: include_exhentai);
                Console.WriteLine("Complete japanese!");
                dbc.ExtractRawDatabase("rawdata-korean", false, "korean", include_exhentai: include_exhentai);
                Console.WriteLine("Complete korean!");

                //dbc.FilterOnlyNewed(sync);
                //dbc.Integrate();
                //var dt = DateTime.Now.ToString("yyyy-MM-dd hh-mm");
                //dbc.ExtractRawDatabase($"chunk/{dt}/rawdata", true);
                //Console.WriteLine("Complete all!");
                //dbc.ExtractRawDatabase($"chunk/{dt}/rawdata-chinese", true, "chinese");
                //Console.WriteLine("Complete chinese!");
                //dbc.ExtractRawDatabase($"chunk/{dt}/rawdata-english", true, "english");
                //Console.WriteLine("Complete english!");
                //dbc.ExtractRawDatabase($"chunk/{dt}/rawdata-japanese", true, "japanese");
                //Console.WriteLine("Complete japanese!");
                //dbc.ExtractRawDatabase($"chunk/{dt}/rawdata-korean", true, "korean");
                //Console.WriteLine("Complete korean!");
            }
            else
            {
                var sync = new SyncronizerLowPerf(hitomi_sync_range, hitomi_sync_lookup_range, exhentai_lookup_page);
                sync.SyncHitomi();
                sync.SyncExHentai();
                sync.FlushToMainDatabase();
                if (use_server) sync.FlushToServerDatabase();

                if (sync_only) return;

                var dbc = new DataBaseCreatorLowPerf();
                dbc.ExtractRawDatabase("rawdata", include_exhentai: include_exhentai);
                Console.WriteLine("Complete all!");
                dbc.ExtractRawDatabase("rawdata-chinese", "chinese", include_exhentai: include_exhentai);
                Console.WriteLine("Complete chinese!");
                dbc.ExtractRawDatabase("rawdata-english", "english", include_exhentai: include_exhentai);
                Console.WriteLine("Complete english!");
                dbc.ExtractRawDatabase("rawdata-japanese", "japanese", include_exhentai: include_exhentai);
                Console.WriteLine("Complete japanese!");
                dbc.ExtractRawDatabase("rawdata-korean", "korean", include_exhentai: include_exhentai);
                Console.WriteLine("Complete korean!");
            }
        }

        static void ProcessCompress(bool include_exhentai, bool low_perf)
        {
            Console.Clear();
            Console.Title = "hsync";

            Console.WriteLine($"hsync - DB Synchronization Manager");
            Console.WriteLine($"Copyright (C) 2020. project violet-server.");
            Console.WriteLine($"Version: {Version.Text} (Build: {Internals.GetBuildDate().ToLongDateString()})");
            Console.WriteLine("");

            if (!low_perf)
            {
                var dbc = new DataBaseCreator();
                dbc.Integrate();
                dbc.ExtractRawDatabase("rawdata", include_exhentai: include_exhentai);
                Console.WriteLine("Complete all!");
                dbc.ExtractRawDatabase("rawdata-chinese", false, "chinese", include_exhentai: include_exhentai);
                Console.WriteLine("Complete chinese!");
                dbc.ExtractRawDatabase("rawdata-english", false, "english", include_exhentai: include_exhentai);
                Console.WriteLine("Complete english!");
                dbc.ExtractRawDatabase("rawdata-japanese", false, "japanese", include_exhentai: include_exhentai);
                Console.WriteLine("Complete japanese!");
                dbc.ExtractRawDatabase("rawdata-korean", false, "korean", include_exhentai: include_exhentai);
                Console.WriteLine("Complete korean!");
            }
            else
            {
                var dbc = new DataBaseCreatorLowPerf();
                dbc.ExtractRawDatabase("rawdata", include_exhentai: include_exhentai);
                Console.WriteLine("Complete all!");
                dbc.ExtractRawDatabase("rawdata-chinese", "chinese", include_exhentai: include_exhentai);
                Console.WriteLine("Complete chinese!");
                dbc.ExtractRawDatabase("rawdata-english", "english", include_exhentai: include_exhentai);
                Console.WriteLine("Complete english!");
                dbc.ExtractRawDatabase("rawdata-japanese", "japanese", include_exhentai: include_exhentai);
                Console.WriteLine("Complete japanese!");
                dbc.ExtractRawDatabase("rawdata-korean", "korean", include_exhentai: include_exhentai);
                Console.WriteLine("Complete korean!");
            }
        }

        static void download_data(string url, string filename)
        {
            Logs.Instance.Push($"Download {filename}...");
            var task = NetTask.MakeDefault(url);

            SingleFileProgressBar pb = null;
            long tsz = 0;
            task.SizeCallback = (sz) =>
            {
                Console.Write("Downloading ... ");
                pb = new SingleFileProgressBar();
                pb.Report(sz, 0);
                tsz = sz;
            };
            task.Filename = filename;
            task.DownloadCallback = (sz) => pb.Report(tsz, sz);
            NetTools.DownloadFile(task);
            pb.Dispose();
            Console.WriteLine("Complete!");
        }

        class xx
        {
            [JsonProperty(PropertyName = "tag")]
            public Dictionary<string, int> tags = new Dictionary<string, int>();
            [JsonProperty(PropertyName = "lang")]
            public Dictionary<string, int> languages = new Dictionary<string, int>();
            [JsonProperty(PropertyName = "artist")]
            public Dictionary<string, int> artists = new Dictionary<string, int>();
            [JsonProperty(PropertyName = "group")]
            public Dictionary<string, int> groups = new Dictionary<string, int>();
            [JsonProperty(PropertyName = "type")]
            public Dictionary<string, int> types = new Dictionary<string, int>();
            [JsonProperty(PropertyName = "uploader")]
            public Dictionary<string, int> uploaders = new Dictionary<string, int>();
            [JsonProperty(PropertyName = "series")]
            public Dictionary<string, int> series = new Dictionary<string, int>();
            [JsonProperty(PropertyName = "character")]
            public Dictionary<string, int> characters = new Dictionary<string, int>();
            [JsonProperty(PropertyName = "class")]
            public Dictionary<string, int> classes = new Dictionary<string, int>();
        }

        static void ProcessRelatedTagTest(string[] args)
        {
            var rtt = new RelatedTagTest(args[0], double.Parse(args[1]));
            rtt.Start();
        }

        static void ProcessCharacterTest(string[] args)
        {
            var rtt = new CharacterTest(args[0], double.Parse(args[1]));
            rtt.Start();
        }

        static void ProcessSeriesTest(string[] args)
        {
            var rtt = new SeriesTest(args[0]);
            rtt.Start();
        }

        static void ProcessCreateEHentaiInverseTable(bool low_perf)
        {
            var index = new List<long>();

            if (!low_perf)
            {
                var ehentaiArticles = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("ex-hentai-archive.json"));

                foreach (var per in ehentaiArticles)
                {
                    var url = per.URL;
                    var xi = Convert.ToInt64(url.Split('/')[4]);
                    var yi = Convert.ToInt64(url.Split('/')[5], 16);
                    index.Add((xi << 40) + yi);
                }
            }
            else
            {
                var db = new SQLiteConnection("data.db");
                var count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM HitomiColumnModel");
                const int perLoop = 50000;

                for (int i = 0; i < count; i += perLoop)
                {
                    var query = db.Query<HitomiColumnModel>($"SELECT * FROM HitomiColumnModel ORDER BY Id LIMIT {perLoop} OFFSET {i}");

                    foreach (var article in query)
                    {
                        if (article.EHash == null) continue;
                        var xi = (long)article.Id;
                        var yi = Convert.ToInt64(article.EHash, 16);
                        index.Add((xi << 40) + yi);
                    }
                }
            }

            if (File.Exists("invtable.json"))
                File.Delete("invtable.json");
            File.WriteAllText("invtable.json", JsonConvert.SerializeObject(index));
        }

        static void ProcessCreateDateTimeEstimator(bool low_perf)
        {
            PolynomialRegressionModel datetimeEstimator = null;
            DateTime mindd;

            var xx1 = new List<double>();
            var yy1 = new List<double>();

            if (!low_perf)
            {
                var ehentaiArticles = JsonConvert.DeserializeObject<List<EHentaiResultArticle>>(File.ReadAllText("ex-hentai-archive.json"));
                mindd = ehentaiArticles.Min(x => DateTime.Parse(x.Published));

                for (int i = 0; i < ehentaiArticles.Count; i++)
                {
                    xx1.Add(int.Parse(ehentaiArticles[i].URL.Split('/')[4]));
                    yy1.Add((DateTime.Parse(ehentaiArticles[i].Published) - mindd).TotalMinutes);
                }
            }
            else
            {
                var db = new SQLiteConnection("data.db");
                var count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM HitomiColumnModel");

                const int perLoop = 50000;

                var dts = new List<DateTime>();

                for (int i = 0; i < count; i += perLoop)
                {
                    var query = db.Query<HitomiColumnModel>($"SELECT * FROM HitomiColumnModel ORDER BY Id LIMIT {perLoop} OFFSET {i}");

                    foreach (var article in query)
                    {
                        if (article.Published.HasValue)
                        {
                            xx1.Add(article.Id);
                            dts.Add(article.Published.Value);
                        }
                    }
                }

                mindd = dts.Min();

                for (int i = 0; i < count; i++)
                {
                    yy1.Add((dts[i] - mindd).TotalMinutes);
                }
            }

            datetimeEstimator = new PolynomialRegressionModel(Vector.Create(yy1.ToArray()), Vector.Create(xx1.ToArray()), 100);
            datetimeEstimator.Fit();
            var poly = datetimeEstimator.GetRegressionPolynomial();
            var x = poly.Parameters.ToList();

            var y = mindd.Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            var jt = new JObject();
            jt.Add("base", y);
            jt.Add("coff", JToken.FromObject(x));

            if (File.Exists("dt-coff.json"))
                File.Delete("dt-coff.json");
            File.WriteAllText("dt-coff.json", jt.ToString());
        }
        
        static void ProcessInitServer(string[] args)
        {
            var db = new SQLiteConnection("data.db");
            var items = db.Query<HitomiColumnModel>("SELECT Id, Files FROM HitomiColumnModel");
            
            using (var conn = new MySqlConnection(Setting.Settings.Instance.Model.ServerConnection))
            {
                conn.Open();

                var myCommand = conn.CreateCommand();
                var transaction = conn.BeginTransaction();

                myCommand.Transaction = transaction;
                myCommand.Connection = conn;

                try
                {
                    myCommand.CommandText = "INSERT INTO article_pages (Id, Pages) VALUES " +
                        string.Join(',', items.Select(x => $"({x.Id}, {x.Files})")) + " ON DUPLICATE KEY UPDATE";
                    myCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception e1)
                    {
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }

}
