using Celeste.Mod;

using PrismaticHelper.Entities;
using PrismaticHelper.Entities.Objects;
using PrismaticHelper.Entities.Panels;
using PrismaticHelper.Entities.Windowpanes;

namespace PrismaticHelper;

public class PrismaticHelperModule : EverestModule {

	public override void Load() {
		Cutscenes.CutsceneTriggers.Load();
		StylegroundsPanelRenderer.Load();
		CassetteKevin.Load();
		Windowpanes.Load();
	}

	public override void Unload() {
		Cutscenes.CutsceneTriggers.Unload();
		StylegroundsPanelRenderer.Unload();
		CassetteKevin.Unload();
		Windowpanes.Unload();
		
		Stencils.Unload();
	}

	public static void LogInfo(string message) {
		Logger.Log(LogLevel.Info, "Prismatic Helper", message);
	}
}