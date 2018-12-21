using System;
using System.Reflection;
using System.Windows;

namespace TankView {
    public partial class AboutPage {
        private bool HasShown;

        public AboutPage() { InitializeComponent(); }

        public AboutPage(Window window) : this() { Owner = window; }

        public string ProgramNameProp => $"TankView v{Assembly.GetExecutingAssembly().GetName().Version}";
        public string TagLineProp     => "TankView uses TankLib, TACTLib, and DataTool.";

        public string DisclaimerL1Prop => "This project is not affiliated with Blizzard Entertainment, Inc.";
        public string DisclaimerL2Prop => "All trademarks referenced herein are the properties of their respective owners.";
        public string DisclaimerL3Prop => $"©{DateTime.Now.Year} Blizzard Entertainment, Inc. All rights reserved.";

        private void ContinueClick(object sender, RoutedEventArgs e) {
            Owner.Show();
            Owner.IsEnabled = true;
            Close();
        }

        private void FirstChance(object sender, EventArgs e) {
            if (HasShown) return;

            HasShown = true;

            ContinueButton.Focus();
        }
    }
}
