using SomethingNeedDoing.Managers;

namespace SomethingNeedDoing;

internal class Service
{
    internal static Plugin Plugin { get; set; } = null!;
    internal static Config Configuration { get; set; } = null!;
    internal static ChatManager ChatManager { get; set; } = null!;
    internal static GameEventManager GameEventManager { get; set; } = null!;
    internal static MacroManager MacroManager { get; set; } = null!;
}
