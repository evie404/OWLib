using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using DataTool;

namespace TankView {
    public partial class DataToolListView : INotifyPropertyChanged {
        public DataToolListView() {
            InitializeComponent();
            var t   = typeof(IAwareTool);
            var asm = t.Assembly;
            var types = asm.GetTypes()
                           .Where(tt => tt.IsClass && t.IsAssignableFrom(tt))
                           .ToList();
            foreach (var tt in types) {
                var attribute = tt.GetCustomAttribute<ToolAttribute>();
                if (tt.IsInterface || attribute == null) continue;

                Tools.Add(new AwareToolEntry(attribute, tt));
            }

            NotifyPropertyChanged(nameof(Tools));
        }

        public List<AwareToolEntry> Tools { get; set; } = new List<AwareToolEntry>();

        public event PropertyChangedEventHandler PropertyChanged;

        private void ActivateTool(object sender, RoutedEventArgs e) {
            var t          = ((AwareToolEntry) ToolList.SelectedItem).Type;
            var tool       = Activator.CreateInstance(t) as IAwareTool;
            var transition = new DataToolProgressTransition(tool);
            transition.Show();
            Close();
        }

        public void NotifyPropertyChanged(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
    }

    public class AwareToolEntry {
        public AwareToolEntry(ToolAttribute attribute, Type tt) {
            Type        = tt;
            Name        = attribute.Name;
            Description = attribute.Description;
            Keyword     = attribute.Keyword;
        }

        public string Name        { get; set; }
        public string Description { get; set; }
        public string Keyword     { get; set; }
        public Type   Type        { get; set; }
    }
}
