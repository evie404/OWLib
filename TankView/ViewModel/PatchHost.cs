using TankView.Properties;

namespace TankView.ViewModel {
    public class PatchHost {
        public PatchHost(string v1, string v2) {
            Host    = v1;
            Name    = v2;
            _active = Settings.Default.NGDPHost == Host;
        }

        public string Host { get; set; }
        public string Name { get; set; }

        private bool _active { get; set; }

        public bool Active {
            get => _active;
            set {
                if (value) {
                    Settings.Default.NGDPHost = Host;
                    Settings.Default.Save();
                }

                _active = value;
            }
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() {
            return (Host?.ToLowerInvariant()
                        ?.GetHashCode()).GetValueOrDefault();
        }
    }
}
