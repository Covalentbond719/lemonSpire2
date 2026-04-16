using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace lemonSpire2.util;

public static class StsUtil
{
    public static T? ResolveModel<T>(string entry) where T : AbstractModel
    {
        return ModelDb.GetByIdOrNull<T>(new ModelId(ModelId.SlugifyCategory<T>(), entry));
    }

    public static string GetPlayerNameFromNetId(ulong netId)
    {
        var runManager = RunManager.Instance;
        return PlatformUtil.GetPlayerName(runManager.NetService.Platform, netId);
    }
}
