using Celeste.Mod;

using PrismaticHelper.Entities;
using PrismaticHelper.Entities.Objects;
using PrismaticHelper.Entities.Panels;
using PrismaticHelper.Triggers;

namespace PrismaticHelper;

public class PrismaticHelperModule : EverestModule {
	public override void Initialize() {
		SpeedrunToolInterop.Initialize();
	}

	public override void Load() {
		Cutscenes.CutsceneTriggers.Load();
		StylegroundsPanelRenderer.Load();
		CassetteListener.Load();
		WorldPanels.Load();
		UnderwaterInteractionTrigger.Load();
	}

	public override void Unload() {
		Cutscenes.CutsceneTriggers.Unload();
		StylegroundsPanelRenderer.Unload();
		CassetteListener.Unload();
		WorldPanels.Unload();
		UnderwaterInteractionTrigger.Unload();

		Stencils.Unload();
	}

	public static void LogError(string message) {
		Logger.Log(LogLevel.Error, "Prismatic Helper", message);
	}
	
	public static void LogInfo(string message) {
		Logger.Log(LogLevel.Info, "Prismatic Helper", message);
	}
}