using System.ComponentModel;

namespace TankView.ViewModel {
    public class ProgressInfo : INotifyPropertyChanged {
        private int    _pc;
        private string _state = "Idle";

        public string State {
            get => _state;
            set {
                _state = value;
                NotifyPropertyChanged(nameof(State));
            }
        }

        public int Percentage {
            get => _pc;
            set {
                _pc = value;
                NotifyPropertyChanged(nameof(Percentage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
    }
}
