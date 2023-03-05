using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
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
		On.Celeste.FancyText.AddWord += TrackPhHighlight;
		On.Celeste.FancyText.Text.Draw += DrawPhHighlight;
		
		On.Celeste.Textbox.ctor_string_Language_Func1Array += AddPhEvents;
		On.Celeste.Textbox.Removed += TextboxOnRemoved;
		
		On.Celeste.Level.SkipCutscene += Level_SkipCutscene;
		On.Celeste.Level.Pause += LevelOnPause;
	}

	public static void Unload(){
		IL.Celeste.FancyText.Parse -= ParsePhTriggers;
		On.Celeste.FancyText.AddWord -= TrackPhHighlight;
		On.Celeste.FancyText.Text.Draw -= DrawPhHighlight;

		On.Celeste.Textbox.ctor_string_Language_Func1Array -= AddPhEvents;
		On.Celeste.Textbox.Removed -= TextboxOnRemoved;

		On.Celeste.Level.SkipCutscene -= Level_SkipCutscene;
		On.Celeste.Level.Pause -= LevelOnPause;
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
				var parserData = new DynamicData(text);
				FancyText.Text group = parserData.Get<FancyText.Text>("group");
				List<FancyText.Node> nodes = group.Nodes;
				switch(s){
					case "ph_trigger":
					case "&ph_trigger":
					case "~ph_trigger":
						nodes.Add(new PhTrigger(vals, s.StartsWith("&"), s.StartsWith("~")));
						break;
					case "ph_on_skip":
						nodes.Add(new PhRunOnSkip(vals));
						break;
					case "ph_highlight":
						parserData.Set("PrismaticHelper:current_highlight", vals.Count == 0 ? null : Calc.HexToColor(vals[0]));
						DynamicData.For(group).Set("PrismaticHelper:has_highlight", true);
						break;
				}
			});
		}
	}
	
	private static void TrackPhHighlight(On.Celeste.FancyText.orig_AddWord orig, FancyText self, string word){
		orig(self, word);
		// skip backslashes, skip space preceded by newlines (how?)
		var parserData = new DynamicData(self);
		parserData.TryGet("PrismaticHelper:current_highlight", out Color? h);
		if(h != null){
			List<FancyText.Node> nodes = parserData.Get<FancyText.Text>("group").Nodes;
			int idx = nodes.Count;
			for(int i = 0; i < word.Replace("\\", "").Length; i++){
				idx--;
				if(idx < 0)
					break;

				if(nodes[idx] is FancyText.Char c)
					DynamicData.For(c).Set("PrismaticHelper:highlight", h);
				else i--; // triggers and stuff don't count towards our total
			}
		}
	}
	
	private static void DrawPhHighlight(On.Celeste.FancyText.Text.orig_Draw orig, FancyText.Text self, Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start, int end){
		var selfData = DynamicData.For(self);
		if(selfData.TryGet("PrismaticHelper:has_highlight", out bool? b) && b == true){
			// find each consecutive group of highlights and draw
			Color? lastColour = null;
			FancyText.Char openingChar = null, closingChar = null;
			for(var idx = start; idx < self.Nodes.Count; idx++){
				var node = self.Nodes[idx];
				if(node is FancyText.Char c){
					DynamicData.For(c).TryGet("PrismaticHelper:highlight", out Color? highlight);
					// highlight is null -> draw? and reset
					// highlight is new and no previous highlight -> start one
					// highlight is same as prev -> expand prev
					// highlight is new and prev exists -> draw and start new
					if(highlight == null){
						if(lastColour != null && openingChar != null && closingChar != null)
							DrawHighlightOn(lastColour.Value, self, start, openingChar, closingChar, position, justify, scale, alpha);
						lastColour = null;
						openingChar = closingChar = null;
					}else if(lastColour == null){
						lastColour = highlight;
						openingChar = closingChar = c;
					}else if(lastColour == highlight){
						closingChar = c;
					}else{
						DrawHighlightOn(lastColour.Value, self, start, openingChar, closingChar, position, justify, scale, alpha);
						lastColour = highlight;
						openingChar = closingChar = c;
					}
				}else if(node is FancyText.NewLine or FancyText.NewPage)
					// hit newline -> draw?
					if(lastColour != null && openingChar != null && closingChar != null){
						DrawHighlightOn(lastColour.Value, self, start, openingChar, closingChar, position, justify, scale, alpha);
						lastColour = null;
						openingChar = closingChar = null;
					}
			}

			// hit end -> draw?
			if(lastColour != null && openingChar != null && closingChar != null)
				DrawHighlightOn(lastColour.Value, self, start, openingChar, closingChar, position, justify, scale, alpha);
		}

		orig(self, position, justify, scale, alpha, start, end);
	}

	private static void DrawHighlightOn(Color highlightColour, FancyText.Text self, int pageStart, FancyText.Char opening, FancyText.Char closing, Vector2 pos, Vector2 justify, Vector2 scale, float alpha){
		// see FancyText::Draw for reference
		if(closing.Fade >= 0.1){
			Vector2 exactPos = pos;
			// figure out the text's actual starting position
			PixelFontSize basePfSize = self.Font.Get(self.BaseSize);
			float maxScale = 0;
			float pageHeight = 0;
			int maxWidth = 0;
			int newlinesBefore = 0;
			bool reachedOpening = false;
			for(int index = pageStart; index < self.Nodes.Count; ++index){
				if(self.Nodes[index] is FancyText.NewLine){
					if(maxScale == 0)
						maxScale = 1;
					pageHeight += maxScale;
					maxScale = 0;
					if(!reachedOpening)
						newlinesBefore++;
				}else if(self.Nodes[index] is FancyText.Char c){
					maxWidth = Math.Max(maxWidth, (int)c.LineWidth);
					maxScale = Math.Max(maxScale, c.Scale);
					if(c == opening)
						reachedOpening = true;
				}else if(self.Nodes[index] is FancyText.NewPage)
					break;
			}
			exactPos -= justify * new Vector2(maxWidth, (maxScale + pageHeight) * basePfSize.LineHeight) * scale;
			exactPos.X += opening.Position * scale.X;
			// and it's actual size
			float localMaxScale = Math.Max(opening.Scale, closing.Scale);
			PixelFontSize exactPfSize = self.Font.Get(self.BaseSize * Math.Max(scale.X, scale.Y) * localMaxScale);
			PixelFontCharacter closingPfChar = exactPfSize.Get(closing.Character);
			var width = closing.Position - opening.Position + closingPfChar.XAdvance;
			var height = basePfSize.LineHeight * scale.Y;
			exactPos.Y += exactPfSize.LineHeight * newlinesBefore;
			// give it some breathing room
			const float padding = 10;
			exactPos.X -= padding;
			width += padding * 2;
			
			Draw.Rect(exactPos, width * closing.Fade, height, highlightColour * closing.Fade * .75f);
		}
	}

	private static void LevelOnPause(On.Celeste.Level.orig_Pause orig, Level self, int startindex, bool minimal, bool quickreset){
		bool wasInCutscene = self.InCutscene, couldRetry = self.CanRetry;
		if(DynamicData.For(self).TryGet("PrismaticHelper:force_unskippable", out bool? noSkip) && noSkip == true){
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