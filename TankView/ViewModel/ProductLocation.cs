namespace TankView.ViewModel {
    public class ProductLocation {
        public ProductLocation(string v1, string v2) {
            Label = v1;
            Value = v2;
        }

        public string Label { get; set; }
        public string Value { get; set; }

        public override int GetHashCode() {
            return Value.ToLowerInvariant()
                        .GetHashCode();
        }
    }
}
