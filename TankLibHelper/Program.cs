using System;
using System.Collections.Generic;
using System.Linq;
using TankLibHelper.Modes;

namespace TankLibHelper {
    internal class Program {
        public static void Main(string[] args) {
            // TankLibHelper createclasses {out} [data dir]
            // TankLibHelper updateclasses {out} [data dir]
            // TankLibHelper testclasses [args]
            // args:
            //     abc.003
            //     *.003
            //     *.003 6A0BTFA(instance hash)

            if (args.Length < 1) {
                Console.Out.WriteLine("Usage: TankLibHelper {mode} [mode args]");
                return;
            }

            var mode = args[0];

            IMode modeObject;
            var   modes = GetModes();

            if (modes.ContainsKey(mode)) {
                modeObject = (IMode) Activator.CreateInstance(modes[mode]);
            } else {
                Console.Out.WriteLine($"Unknown mode: {mode}");
                Console.Out.WriteLine("Valid modes are:");
                foreach (var modeName in modes.Keys) Console.Out.WriteLine($"    {modeName}");
                return;
            }

            var result = modeObject.Run(args);

            if (result == ModeResult.Fail)
                Console.Out.WriteLine($"\r\n{mode} failed to execute successfully");
            else if (result == ModeResult.Success) Console.Out.WriteLine("\r\nDone");
        }

        public static Dictionary<string, Type> GetModes() {
            var modes = new Dictionary<string, Type>();
            foreach (var modeType in typeof(IMode).Assembly.GetTypes()
                                                  .Where(x => typeof(IMode).IsAssignableFrom(x))) {
                if (modeType.IsInterface) continue;
                var inst = (IMode) Activator.CreateInstance(modeType);
                modes[inst.Mode] = modeType;
            }

            return modes;
        }
    }
}
