using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace lemonSpire2.Tooltips;

/// <summary>
///     Base class for serializable tooltips.
///     Implementations should provide ToHoverTip() for UI rendering.
/// </summary>
public abstract class Tooltip
{
    private static int _nextRegistryId = 1;
    private static readonly Dictionary<int, WeakReference<Tooltip>> Registry = new();
    
    public static readonly int FontSize = 18;

    protected Tooltip()
    {
        RegistryId = _nextRegistryId++;
        Registry[RegistryId] = new WeakReference<Tooltip>(this);
    }

    protected abstract string TypeTag { get; }

    /// <summary>
    ///     Internal registry ID used for meta string lookup.
    /// </summary>
    public int RegistryId { get; }

    public abstract string Render();
    
    public abstract void Serialize(PacketWriter writer);
    public abstract void Deserialize(PacketReader reader);

    /// <summary>
    ///     Converts this tooltip to an IHoverTip for rendering by NHoverTipSet.
    /// </summary>
    public abstract IHoverTip ToHoverTip();

    public static Tooltip? TryResolve(int registryId)
    {
        if (!Registry.TryGetValue(registryId, out var weak)) return null;

        if (weak.TryGetTarget(out var tooltip))
            return tooltip;

        Registry.Remove(registryId);
        return null;
    }

    public static void Cleanup()
    {
        var deadKeys = new List<int>();
        foreach (var (id, weak) in Registry)
            if (!weak.TryGetTarget(out _))
                deadKeys.Add(id);
        foreach (var id in deadKeys)
            Registry.Remove(id);
    }

    public string ToMetaString()
    {
        return $"{TypeTag}:{RegistryId}";
    }

    public static Tooltip? FromMetaString(string meta)
    {
        var span = meta.AsSpan();
        var colonIndex = span.IndexOf(':');
        if (colonIndex < 0) return null;

        var idSpan = span[(colonIndex + 1)..];
        return !int.TryParse(idSpan, out var id) ? null : TryResolve(id);
    }
}
