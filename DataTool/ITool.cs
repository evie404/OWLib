using DragonLib.CLI;

namespace DataTool {
    public interface ITool {
        void Parse(ICLIFlags toolFlags);
    }
}
