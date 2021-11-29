using Celeste.Mod;

namespace PrismaticHelper {

	public class PrismaticHelperModule : EverestModule {

		public override void Load() {
			Cutscenes.CutsceneTriggers.Load();
		}

		public override void Unload() {
			Cutscenes.CutsceneTriggers.Unload();
		}

		public static void LogInfo(string message) {
			Logger.Log(LogLevel.Info, "Prismatic Helper", message);
		}
	}
}
