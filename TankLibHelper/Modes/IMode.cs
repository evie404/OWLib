namespace TankLibHelper.Modes {
    public interface IMode {
        string     Mode { get; }
        ModeResult Run(string[] args);
    }
}
