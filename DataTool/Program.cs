﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DataTool.ConvertLogic;
using DragonLib.CLI;
using DataTool.Helper;
using DataTool.SaveLogic;
using Microsoft.Win32;
using TankLib;
using TankLib.STU;
using TankLib.STU.Types;
using TankLib.TACT;
using TACTLib.Client;
using TACTLib.Client.HandlerArgs;
using TACTLib.Core.Product.Tank;
using TACTLib.Exceptions;
using TankLib.Helpers;
using static DataTool.Helper.Logger;
using static DataTool.Helper.STUHelper;
using Logger = TankLib.Helpers.Logger;
using static DataTool.Helper.SpellCheckUtils;

namespace DataTool {
    public static class Program {
        public static ClientHandler Client;
        public static ProductHandler_Tank TankHandler;
        public static Dictionary<ushort, HashSet<ulong>> TrackedFiles;

        public static ToolFlags Flags;
        public static uint BuildVersion;
        public static bool IsPTR => Client?.AgentProduct?.Uid == "prometheus_test";

        public static string[] ValidLanguages = {"deDE", "enUS", "esES", "esMX", "frFR", "itIT", "jaJP", "koKR", "plPL", "ptBR", "ruRU", "zhCN", "zhTW"};

        public static bool ValidKey(ulong key) {
            return TankHandler.m_assets.ContainsKey(key);
        }

        public static HashSet<Type> GetTools(bool noDebugTools = false) {
            var tools = new HashSet<Type>();
            {
                var t = typeof(ITool);
                var asm = t.Assembly;
                var types = asm.GetTypes()
                    .Where(tt => tt.IsClass && t.IsAssignableFrom(tt))
                    .ToList();
                foreach (var tt in types) {
                    var attribute = tt.GetCustomAttribute<ToolAttribute>();
                    if (tt.IsInterface || attribute == null) continue;

                    tools.Add(tt);
                }
            }
            return tools;
        }

        private static void Main() {
            InitTankSettings();

            HookConsole();

            var tools = GetTools();

            var args = Environment.GetCommandLineArgs().SkipWhile(x => x.EndsWith(".exe") || x.EndsWith(".dll")).ToArray();
            Logger.Info("Core", $"{Assembly.GetExecutingAssembly().GetName().Name} v{Util.GetVersion(typeof(Program).Assembly)}");

            Logger.Info("Core", $"CommandLine: [{string.Join(", ", args)}]");

            Flags = CommandLineFlags.ParseFlags<ToolFlags>((flags, invoked) => {
                CommandLineFlags.PrintHelp(flags, invoked);
                PrintHelp(tools, invoked);
            }, args);
            if (Flags == null)
                return;


            if (string.IsNullOrWhiteSpace(Flags.OverwatchDirectory) || string.IsNullOrWhiteSpace(Flags.Mode)) {
                PrintHelp(tools, true);
                return;
            }

            ITool targetTool = null;
            ICLIFlags targetToolFlags = null;
            ToolAttribute targetToolAttributes = null;

            #region Tool Activation

            foreach (var type in tools) {
                var attribute = type.GetCustomAttribute<ToolAttribute>();

                if (!string.Equals(attribute.Keyword, Flags.Mode, StringComparison.InvariantCultureIgnoreCase)) continue;
                targetTool = Activator.CreateInstance(type) as ITool;
                targetToolAttributes = attribute;

                if (attribute.CustomFlags != null) {
                    var flags = attribute.CustomFlags;
                    if (typeof(ICLIFlags).IsAssignableFrom(flags))
                        targetToolFlags = typeof(CommandLineFlags).GetMethod(nameof(CommandLineFlags.ParseFlags), new [] { typeof(string[]) })
                                              ?.MakeGenericMethod(flags)
                                              .Invoke(null, null) as ICLIFlags;
                }

                break;
            }

            if (targetToolFlags == null && targetTool != null) {
                return;
            }

            if (targetTool == null) {
                PrintHelp(tools, true);
                return;
            }

            #endregion

            if (!targetToolAttributes.UtilNoArchiveNeeded) {
                try {
                    InitStorage(Flags.Online);
                } catch (Exception ex) when (ex.InnerException is UnsupportedBuildVersionException) {
                    Logger.Log24Bit(ConsoleSwatch.XTermColor.OrangeRed, true, Console.Error, "CASC", "This version of DataTool does not support this version of Overwatch. Download a newer version of the tools.");
                    throw;
                }
                catch {
                    Logger.Log24Bit(ConsoleSwatch.XTermColor.OrangeRed, true, Console.Error, "CASC",
                                    "=================\nError initializing CASC!\n" +
                                    "Please Scan & Repair your game, launch it for a minute, and try the tools again before reporting a bug!\n" +
                                    "========================");
                    throw;
                }

                //foreach (KeyValuePair<ushort, HashSet<ulong>> type in TrackedFiles.OrderBy(x => x.Key)) {
                //    //Console.Out.WriteLine($"Found type: {type.Key:X4} ({type.Value.Count} files)");
                //    Console.Out.WriteLine($"Found type: {type.Key:X4}");
                //}

                InitKeys();
                InitMisc();
            }

            var stopwatch = new Stopwatch();
            Logger.Info("Core", "Tooling...");
            stopwatch.Start();
            targetTool.Parse(targetToolFlags);
            stopwatch.Stop();

            Logger.Success("Core", $"Execution finished in {stopwatch.Elapsed} seconds");

            ShutdownMisc();
        }

        private static void HookConsole() {
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
            Process.GetCurrentProcess()
                .EnableRaisingEvents = true;
            AppDomain.CurrentDomain.ProcessExit += (sender, @event) => Console.ForegroundColor = ConsoleColor.Gray;
            Console.CancelKeyPress += (sender, @event) => Console.ForegroundColor = ConsoleColor.Gray;
            Console.OutputEncoding = Encoding.UTF8;
        }

        private static void InitTankSettings() {
            Logger.ShowDebug = Debugger.IsAttached;
        }

        public static void InitMisc() {
            var dbPath = Flags.ScratchDBPath;
            if (Flags.Deduplicate) {
                Logger.Warn("ScratchDB", "Will attempt to deduplicate files if extracting...");
                if (!string.IsNullOrWhiteSpace(Flags.ScratchDBPath)) {
                    Logger.Warn("ScratchDB", "Loading Scratch database...");
                    if (!File.Exists(dbPath) || new DirectoryInfo(dbPath).Exists)
                        dbPath = Path.Combine(Path.GetFullPath(Flags.ScratchDBPath), "Scratch.db");

                    Combo.ScratchDBInstance.Load(dbPath);
                }
            }

            if (!Flags.NoGuidNames)
                IO.LoadGUIDTable(Flags.OnlyCanonical);

            IO.LoadLocalizedNamesMapping();

            Sound.WwiseBank.GetReady();
        }

        public static void ShutdownMisc() {
            SaveScratchDatabase();
        }

        public static void SaveScratchDatabase() {
            if (!string.IsNullOrWhiteSpace(Flags.ScratchDBPath)) {
                var dbPath = Flags.ScratchDBPath;
                if (!File.Exists(dbPath) || new DirectoryInfo(dbPath).Exists)
                    dbPath = Path.Combine(Path.GetFullPath(Flags.ScratchDBPath), "Scratch.db");

                if (Flags.Deduplicate && !string.IsNullOrWhiteSpace(dbPath)) {
                    Logger.Warn("ScratchDB", "Saving Scratch database...");
                    Combo.ScratchDBInstance.Save(dbPath);
                }
            }
        }

        public static void InitStorage(bool online = false) { // turnin offline off again, can cause perf issues with bundle hack
            // Attempt to load language via registry, if they were already provided via flags then this won't do anything
            if (!Flags.NoLanguageRegistry)
                TryFetchLocaleFromRegistry();

            Logger.Info("CASC", $"Text Language: {Flags.Language} | Speech Language: {Flags.SpeechLanguage}");

            ManifestCryptoHandler.AttemptFallbackManifests = Flags.TryManifestFallback;
            var args = new ClientCreateArgs {
                SpeechLanguage = Flags.SpeechLanguage,
                TextLanguage = Flags.Language,
                HandlerArgs = new ClientCreateArgs_Tank {CacheAPM = Flags.UseCache, ManifestRegion = Flags.RCN ? ProductHandler_Tank.REGION_CN : ProductHandler_Tank.REGION_DEV},
                Online = online
            };

            LoadHelper.PreLoad();
            Client = new ClientHandler(Flags.OverwatchDirectory, args);
            LoadHelper.PostLoad(Client);

            if (args.TextLanguage != "enUS")
                Logger.Warn("Core", "Reminder! When extracting data in other languages, the names of the heroes/skins/etc must be in the language you have chosen.");

            if (Client.AgentProduct.ProductCode != "pro")
                Logger.Warn("Core", $"The branch \"{Client.AgentProduct.ProductCode}\" is not supported!. This might result in failure to load. Proceed with caution.");

            var clientLanguages = Client.AgentProduct.Settings.Languages.Select(x => x.Language).ToArray();
            if (!clientLanguages.Contains(args.TextLanguage))
                Logger.Warn("Core", "Battle.Net Agent reports that text language {0} is not installed.", args.TextLanguage);
            else if (!clientLanguages.Contains(args.SpeechLanguage))
                Logger.Warn("Core", "Battle.Net Agent reports that speech language {0} is not installed.", args.SpeechLanguage);

            TankHandler = Client.ProductHandler as ProductHandler_Tank;
            if (TankHandler == null) {
                Logger.Error("Core", $"Not a valid Overwatch installation (detected product: {Client.Product})");
                return;
            }

            BuildVersion = uint.Parse(Client.InstallationInfo.Values["Version"].Split('.').Last());
            if (BuildVersion < 39028)
                Logger.Error("Core", "DataTool doesn't support Overwatch versions below 1.14. Please use OverTool.");
            else if (BuildVersion < ProductHandler_Tank.VERSION_152_PTR)
                Logger.Error("Core", "This version of DataTool doesn't support versions of Overwatch below 1.52. Please downgrade DataTool.");

            InitTrackedFiles();
        }

        public static void InitTrackedFiles() {
            TrackedFiles = new Dictionary<ushort, HashSet<ulong>>();
            foreach (var asset in TankHandler.m_assets) {
                var type = teResourceGUID.Type(asset.Key);
                if (!TrackedFiles.TryGetValue(type, out var typeMap)) {
                    typeMap = new HashSet<ulong>();
                    TrackedFiles[type] = typeMap;
                }

                typeMap.Add(asset.Key);
            }
        }

        public static void InitKeys() {
            if (!Flags.SkipKeys) {
                Logger.Info("Core", "Checking ResourceKeys");

                foreach (var key in TrackedFiles[0x90]) {
                    if (!ValidKey(key)) continue;

                    var resourceKey = GetInstance<STUResourceKey>(key);
                    if (resourceKey == null || resourceKey.GetKeyID() == 0 || Client.ConfigHandler.Keyring.Keys.ContainsKey(resourceKey.GetReverseKeyID())) continue;
                    Client.ConfigHandler.Keyring.AddKey(resourceKey.GetReverseKeyID(), resourceKey.m_key);
                    Logger.Info("Core", $"Added ResourceKey {resourceKey.GetKeyIDString()}, Value: {resourceKey.GetKeyValueString()}");
                }
            }
        }

        private static void TryFetchLocaleFromRegistry() {
            try {
                if (Flags.Language == null) {
                    var textLanguage = (string) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Blizzard Entertainment\Battle.net\Launch Options\Pro", "LOCALE", null);
                    if (!string.IsNullOrWhiteSpace(textLanguage)) {
                        if (ValidLanguages.Contains(textLanguage))
                            Flags.Language = textLanguage;
                        else
                            Logger.Error("Core", $"Invalid text language found via registry: {textLanguage}. Ignoring.");
                    }
                }

                if (Flags.SpeechLanguage == null) {
                    var speechLanguage = (string) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Blizzard Entertainment\Battle.net\Launch Options\Pro", "LOCALE_AUDIO", null);
                    if (!string.IsNullOrWhiteSpace(speechLanguage)) {
                        if (ValidLanguages.Contains(speechLanguage))
                            Flags.SpeechLanguage = speechLanguage;
                        else
                            Logger.Error("Core", $"Invalid speech language found via registry: {speechLanguage}. Ignoring.");
                    }
                }
            } catch (Exception) {
                // Ignored
            }
        }

        private static void HandleSingleException(Exception ex) {
            if (ex is TargetInvocationException fex) {
                ex = fex.InnerException ?? ex;
            }

            Logger.Log24Bit(ConsoleSwatch.XTermColor.HotPink3, true, Console.Error, null, ex.Message);
            Logger.Log24Bit(ConsoleSwatch.XTermColor.MediumPurple, true, Console.Error, null, ex.StackTrace);

            if (ex is BLTEDecoderException decoder) {
                File.WriteAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"BLTEDump-{AppDomain.CurrentDomain.FriendlyName}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.blte"), decoder.GetBLTEData());
            }

            if (ex.InnerException != null) {
                HandleSingleException(ex.InnerException);
            }
        }

        [DebuggerStepThrough]
        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs e) {
            if (e.ExceptionObject is Exception ex) {
                HandleSingleException(ex);

                if (Debugger.IsAttached) throw ex;
            }

            unchecked {
                Environment.Exit(-1);
            }
        }

        private static void PrintHelp(IEnumerable<Type> eTools, bool invoked) {
            var tools = new List<Type>(eTools);
            tools.Sort(new ToolComparer());
            Log();
            Log("Modes:");
            Log("  {0, -26} | {1, -40}", "mode", "description");
            Log("".PadLeft(94, '-'));
            foreach (var t in tools) {
                var attribute = t.GetCustomAttribute<ToolAttribute>();
                if (attribute.IsSensitive && !Debugger.IsAttached) continue;

                var desc = attribute.Description;
                if (attribute.Description == null) desc = "";

                Log("  {0, -26} | {1}", attribute.Keyword, desc);
            }

            var sortedTools = new Dictionary<string, List<Type>>();

            foreach (var t in tools) {
                var attribute = t.GetCustomAttribute<ToolAttribute>();
                if (attribute.IsSensitive) continue;
                if (attribute.Keyword == null) continue;
                if (!attribute.Keyword.Contains("-")) continue;

                var result = attribute.Keyword.Split('-')
                    .First();

                if (!sortedTools.ContainsKey(result)) sortedTools[result] = new List<Type>();
                sortedTools[result]
                    .Add(t);
            }

            foreach (var toolType in sortedTools) {
                var firstTool = toolType.Value.FirstOrDefault();
                var attribute = firstTool?.GetCustomAttribute<ToolAttribute>();
                if (attribute?.CustomFlags == null) continue;
                var flags = attribute.CustomFlags;
                if (!typeof(ICLIFlags).IsAssignableFrom(attribute.CustomFlags)) continue;
                Log();
                Log("Flags for {0}-*", toolType.Key);
                var flagList = flags.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty).Select(x => (x.GetCustomAttribute<CLIFlagAttribute>(true), x.PropertyType)).ToList();
                CommandLineFlags.PrintHelp(flagList, true);
            }
            ToolNameSpellCheck();
        }

        internal class ToolComparer : IComparer<Type> {
            public int Compare(Type x, Type y) {
                var xT = (x ?? throw new ArgumentNullException(nameof(x))).GetCustomAttribute<ToolAttribute>();
                var yT = (y ?? throw new ArgumentNullException(nameof(y))).GetCustomAttribute<ToolAttribute>();
                return string.Compare(xT.Keyword, yT.Keyword, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private static void ToolNameSpellCheck() {
            //this will happen if mode is not found
            if (string.IsNullOrWhiteSpace(Flags?.Mode?.ToLower())) {
                return;
            }
            var symSpell = new SymSpell(50, 6);
            FillToolSpellDict(symSpell);
            SpellCheckString(Flags.Mode.ToLower(), symSpell);
        }
    }
}
