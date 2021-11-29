using System;

using Celeste.Mod;

namespace PrismaticHelper {

	public class PrismaticHelperModule : EverestModule {

		public override void Load() {
			
		}

		public override void Unload() {
			
		}

		public static void LogInfo(string message) {
			Logger.Log(LogLevel.Info, "Prismatic Helper", message);
		}
	}
}
