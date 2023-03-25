using Celeste.Mod;

using PrismaticHelper.Entities;
using PrismaticHelper.Entities.Panels;

namespace PrismaticHelper;

public class PrismaticHelperModule : EverestModule {
	public override void Initialize() {
		SpeedrunToolInterop.Initialize();
	}

	public override void Load() {
		Cutscenes.CutsceneTriggers.Load();
		StylegroundsPanelRenderer.Load();
		CassetteListener.Load();
		Windowpanes.Load();
	}

	public override void Unload() {
		Cutscenes.CutsceneTriggers.Unload();
		StylegroundsPanelRenderer.Unload();
		CassetteListener.Unload();
		Windowpanes.Unload();

		Stencils.Unload();
	}

	public static void LogInfo(string message) {
		Logger.Log(LogLevel.Info, "Prismatic Helper", message);
	}
}