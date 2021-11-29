using Celeste;
using Celeste.Mod;

using Mono.Cecil.Cil;

using Monocle;

using MonoMod.Cil;
using MonoMod.Utils;

using System;
using System.Collections.Generic;

namespace PrismaticHelper.Cutscenes {

	public class ParserHooks {

		public class PhTrigger : FancyText.Trigger {

			public string ID;

			public List<string> Params;

			public PhTrigger(List<string> rawParams, bool silent) {
				Silent = silent;

				if(rawParams.Count == 0) {
					Logger.Log(LogLevel.Warn, "PrismaticHelper", "Found empty ph_trigger!");
				} else {
					ID = rawParams[0];
					Params = rawParams.GetRange(1, rawParams.Count - 1);
				}
			}
		}

		public static void LoadHooks() {
			IL.Celeste.FancyText.Parse += ParsePhTriggers;
			On.Celeste.Textbox.ctor_string_Language_Func1Array += AddPhEvents;
		}

		public static void Unload() {
			IL.Celeste.FancyText.Parse -= ParsePhTriggers;
			On.Celeste.Textbox.ctor_string_Language_Func1Array -= AddPhEvents;
		}

		private static void AddPhEvents(On.Celeste.Textbox.orig_ctor_string_Language_Func1Array orig, Textbox self, string dialog, Language language, Func<System.Collections.IEnumerator>[] events) {
			orig(self, dialog, language, events);
			var selfData = new DynamicData(self);
			var text = selfData.Get<FancyText.Text>("text");
			int vanillaCount = events.Length; // This should never be >0 with just Prismatic Helper, but other mods may want to use vanilla events like this.
			var phEvents = new List<Func<System.Collections.IEnumerator>>();

			foreach(var node in text.Nodes)
				if(node is PhTrigger ph) {
					ph.Index = vanillaCount + phEvents.Count;
					Level level = Engine.Scene as Level;
					phEvents.Add(CutsceneTriggers.Get(ph.ID, level.Tracker.GetEntity<Player>(), level, ph.Params));
				}

			var newEvents = new Func<System.Collections.IEnumerator>[vanillaCount + phEvents.Count];
			Array.Copy(events, newEvents, vanillaCount);
			for(int i = 0; i < phEvents.Count; i++)
				newEvents[i + vanillaCount] = phEvents[i];

			selfData.Set("events", newEvents);
		}

		private static void ParsePhTriggers(ILContext il) {
			var cursor = new ILCursor(il);
			if(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdstr("savedata"))) {
				PrismaticHelperModule.LogInfo("Hooking into FancyText.Parse for Prismatic Helper cutscenes!");
				cursor.Emit(OpCodes.Ldarg_0); // this
				cursor.Emit(OpCodes.Ldloc_S, il.Method.Body.Variables[7]); // s
				cursor.Emit(OpCodes.Ldloc_S, il.Method.Body.Variables[8]); // stringList
				cursor.EmitDelegate<Action<FancyText, string, List<string>>>((text, s, vals) => {
					if(s.Equals("ph_trigger") || s.Equals("&ph_trigger"))
						new DynamicData(text).Get<FancyText.Text>("group").Nodes.Add(new PhTrigger(vals, s.StartsWith("&")));
				});
			}
		}
	}
}
