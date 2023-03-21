using System;
using Monocle;
using MonoMod.ModInterop;

namespace PrismaticHelper.Entities;

[ModExportName("Celeste.Mod.SpeedrunTool.SpeedrunToolInterop")]
public static class SpeedrunToolInterop {
    public static Action<Entity, bool> IgnoreSaveState;

    public static void Initialize() {
        typeof(SpeedrunToolInterop).ModInterop();
    }
}