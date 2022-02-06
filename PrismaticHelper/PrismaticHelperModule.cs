using Celeste.Mod;

using PrismaticHelper.Entities;

namespace PrismaticHelper {

	public class PrismaticHelperModule : EverestModule {

		public override void Load() {
			Cutscenes.CutsceneTriggers.Load();
			StylegroundsPanelRenderer.Load();
		}

		public override void Unload() {
			Cutscenes.CutsceneTriggers.Unload();
			StylegroundsPanelRenderer.Unload();
		}

		public static void LogInfo(string message) {
			Logger.Log(LogLevel.Info, "Prismatic Helper", message);
		}
	}
}
