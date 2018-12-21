using System.Collections.Generic;
using System.IO;
using TankView.ObjectModel;
using TACTLib.Agent;

namespace TankView.ViewModel {
    public class ProductLocations : ObservableHashCollection<ProductLocation> {
        private static Dictionary<string, string> KnownUIDs = new Dictionary<string, string> {
                                                                                                 { "prometheus", "Live" },
                                                                                                 { "prometheus_dev", "Development" },
                                                                                                 { "prometheus_test", "Public Test" },
                                                                                                 { "prometheus_tournament", "Professional" },
                                                                                                 { "prometheus_viewer", "Viewer" }
                                                                                             };

        public ProductLocations() {
            try {
                var pdb = new AgentDatabase();
                foreach (var install in pdb.Data.ProductInstalls)
                    if (KnownUIDs.ContainsKey(install.Uid) && Directory.Exists(install.Settings.InstallPath))
                        Add(new ProductLocation(KnownUIDs[install.Uid], Path.GetFullPath(install.Settings.InstallPath)));
            } catch { }
        }
    }
}
