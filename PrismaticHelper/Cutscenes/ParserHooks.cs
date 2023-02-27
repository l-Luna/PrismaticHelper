using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace PrismaticHelper.Cutscenes;

public static class ParserHooks{
	
	public class PhTrigger : FancyText.Trigger{
		
		public readonly string ID = "";

		public readonly List<string> Params = new();

		// Whether the trigger should run alongside dialog rather than block it.
		public readonly bool Concurrent;

		public PhTrigger(List<string> rawParams, bool silent, bool concurrent){
			Silent = silent;
			Concurrent = concurrent;

			if(rawParams.Count == 0){
				Logger.Log(LogLevel.Warn, "PrismaticHelper", "Found empty ph_trigger!");
			}else{
				ID = rawParams[0];
				Params = rawParams.GetRange(1, rawParams.Count - 1);
			}
		}
	}

	public class PhRunOnSkip : FancyText.Node{
		
		public readonly string ID = "";

		public readonly List<string> Params = new();

		public PhRunOnSkip(List<string> rawParams){
			if(rawParams.Count == 0){
				Logger.Log(LogLevel.Warn, "PrismaticHelper", "Found empty ph_on_skip!");
			}else{
				ID = rawParams[0];
				Params = rawParams.GetRange(1, rawParams.Count - 1);
			}
		}
	}

	public static void LoadHooks(){
		IL.Celeste.FancyText.Parse += ParsePhTriggers;
		On.Celeste.Textbox.ctor_string_Language_Func1Array += AddPhEvents;
		On.Celeste.Level.SkipCutscene += Level_SkipCutscene;
		On.Celeste.Level.Pause += LevelOnPause;
		On.Celeste.Textbox.Removed += TextboxOnRemoved;
	}

	public static void Unload(){
		IL.Celeste.FancyText.Parse -= ParsePhTriggers;
		On.Celeste.Textbox.ctor_string_Language_Func1Array -= AddPhEvents;
		On.Celeste.Level.SkipCutscene -= Level_SkipCutscene;
		On.Celeste.Level.Pause -= LevelOnPause;
		On.Celeste.Textbox.Removed -= TextboxOnRemoved;
	}

	private static void AddPhEvents(On.Celeste.Textbox.orig_ctor_string_Language_Func1Array orig, Textbox self, string dialog, Language language, Func<System.Collections.IEnumerator>[] events){
		orig(self, dialog, language, events);
		var selfData = new DynamicData(self);
		var text = selfData.Get<FancyText.Text>("text");
		events ??= new Func<System.Collections.IEnumerator>[0];
		int vanillaCount = events.Length; // This should never be >0 with just Prismatic Helper, but other mods may want to use vanilla events like this.
		var phEvents = new List<Func<System.Collections.IEnumerator>>();

		foreach(var node in text.Nodes)
			if(node is PhTrigger ph){
				ph.Index = vanillaCount + phEvents.Count;
				Level level = Engine.Scene as Level;
				var cutscene = CutsceneTriggers.Get(ph.ID, level.Tracker.GetEntity<Player>(), level, ph.Params);
				var copy = cutscene; // avoid cutscene referencing itself
				if(ph.Concurrent)
					cutscene = () => WrapCoroutine(copy());
				phEvents.Add(cutscene);
			}

		var newEvents = new Func<System.Collections.IEnumerator>[vanillaCount + phEvents.Count];
		Array.Copy(events, newEvents, vanillaCount);
		for(int i = 0; i < phEvents.Count; i++)
			newEvents[i + vanillaCount] = phEvents[i];

		selfData.Set("events", newEvents);
	}

	private static void Level_SkipCutscene(On.Celeste.Level.orig_SkipCutscene orig, Level self){
		// Assume there is only one textbox entity, or that all textbox entities are being closed
		var player = self.Tracker.GetEntity<Player>();
		self.Entities.With<Textbox>(textbox => {
			DynamicData boxData = new(textbox);
			List<FancyText.Node> nodes = boxData.Get<FancyText.Text>("text").Nodes;
			for(int i = 0; i < nodes.Count; i++){
				FancyText.Node node = nodes[i];
				if(node is PhRunOnSkip skip){
					var cutscene = CutsceneTriggers.Get(skip.ID, player, self, skip.Params)();
					while(cutscene.MoveNext())
						; // no delay between actions
				}
			}
		});

		CutsceneTriggers.CleanupOnSkip(self, player);
		orig(self);
	}

	private static void ParsePhTriggers(ILContext il){
		var cursor = new ILCursor(il);
		if(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdstr("savedata"))){
			PrismaticHelperModule.LogInfo("Hooking into FancyText.Parse for Prismatic Helper cutscenes!");
			cursor.Emit(OpCodes.Ldarg_0); // this
			cursor.Emit(OpCodes.Ldloc_S, il.Method.Body.Variables[7]); // s
			cursor.Emit(OpCodes.Ldloc_S, il.Method.Body.Variables[8]); // stringList
			cursor.EmitDelegate<Action<FancyText, string, List<string>>>((text, s, vals) => {
				List<FancyText.Node> nodes = new DynamicData(text).Get<FancyText.Text>("group").Nodes;
				if(s.Equals("ph_trigger") || s.Equals("&ph_trigger") || s.Equals("~ph_trigger"))
					nodes.Add(new PhTrigger(vals, s.StartsWith("&"), s.StartsWith("~")));
				if(s.Equals("ph_on_skip"))
					nodes.Add(new PhRunOnSkip(vals));
			});
		}
	}
	
	private static void LevelOnPause(On.Celeste.Level.orig_Pause orig, Level self, int startindex, bool minimal, bool quickreset){
		bool wasInCutscene = self.InCutscene, couldRetry = self.CanRetry;
		if(new DynamicData(self).TryGet("PrismaticHelper:force_unskippable", out bool? noSkip) && noSkip == true){
			self.InCutscene = false;
			self.CanRetry = false;
		}
		orig(self, startindex, minimal, quickreset);
		self.InCutscene = wasInCutscene;
		self.CanRetry = couldRetry;
	}
	
	private static void TextboxOnRemoved(On.Celeste.Textbox.orig_Removed orig, Textbox self, Scene scene){
		new DynamicData(scene).Set("PrismaticHelper:force_unskippable", false);
		orig(self, scene);
	}

	public static System.Collections.IEnumerator WrapCoroutine(System.Collections.IEnumerator orig){
		Entity entity = new();
		entity.Add(new Coroutine(orig));
		Engine.Scene.Add(entity);
		yield return null;
	}
}