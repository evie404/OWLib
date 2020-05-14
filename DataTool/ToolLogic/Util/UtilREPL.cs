using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataTool.Flag;
using DataTool.Helper;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataTool.ToolLogic.Util {
    [Tool("repl", CustomFlags = typeof(ToolFlags), Description = "Read, Eval, Print, Loop")]
    public class UtilREPL : ITool {
        public void Parse(ICLIFlags toolFlags) {
            Task.WaitAll(Task.Run(async () => {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                var scriptSettings = ScriptOptions.Default.WithAllowUnsafe(true).AddReferences(GetType().Assembly).AddReferences(GetType().Assembly.GetReferencedAssemblies().Select(Assembly.Load));

                var state = await CSharpScript.RunAsync("using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing System.IO;\nusing static DataTool.Program;\nusing TankLib;\nusing TankLib.Math;\nusing TankLib.STU.Types;\nusing TankLib.STU.Types.Enums;\nusing static DataTool.Helper.IO;\nusing static DataTool.Helper.STUHelper;", scriptSettings);
                while (true) {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (input == "quit") break;
                    switch (input) {
                        case "load-type-assembly":
                            try {
                                scriptSettings.AddReferences(Type.GetType(input.Split(' ')[1])?.Assembly);
                            } catch (Exception e) {
                                Logger.ErrorLog(e.Message);
                            }

                            continue;
                        case "dump-script": {
                            var parts = input.Split(new [] { ' '}, 2, StringSplitOptions.RemoveEmptyEntries);
                            var dumpScript = "DataToolREPL.cs";
                            if (parts.Length > 1) {
                                dumpScript = parts[1];
                            }

                            var script = state.Script;

                            var lines = new List<string>();
                            while (script != null) {
                                lines.Insert(0, script.Code);
                                script = script.Previous;
                            }
                            File.WriteAllText(dumpScript, string.Join(Environment.NewLine, lines));
                            continue;
                        }
                        case "load-script": {
                            var parts = input.Split(new[] {' '}, 2, StringSplitOptions.RemoveEmptyEntries);
                            var dumpScript = "DataToolREPL.cs";
                            if (parts.Length > 1) {
                                dumpScript = parts[1];
                            }

                            try {
                                state = await CSharpScript.RunAsync(File.ReadAllText(dumpScript), scriptSettings);
                            } catch (Exception e) {
                                Logger.ErrorLog(e.Message);
                                continue;
                            }

                            break;
                        }
                        default:
                            try {
                                state = await state.ContinueWithAsync(input, scriptSettings);
                            } catch (Exception e) {
                                Logger.ErrorLog(e.Message);
                                continue;
                            }

                            break;
                    }

                    if (state.ReturnValue == null) continue;

                    Console.WriteLine(JsonConvert.SerializeObject(state.ReturnValue, Formatting.Indented, settings));
                }
            }));
        }
    }
}
