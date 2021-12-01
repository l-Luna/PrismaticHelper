using System;
using System.Collections;
using System.Collections.Generic;

using Celeste;
using Celeste.Mod.Meta;

using Microsoft.Xna.Framework;

using Mono.Cecil.Cil;

using Monocle;

using MonoMod.Cil;

namespace PrismaticHelper.Cutscenes {

	public static class CutsceneTriggers {

		public static readonly Dictionary<string, Func<Player, Level, List<string>, IEnumerator>> Triggers = new();

		public static void Load() {
			ParserHooks.LoadHooks();

			static IEnumerator nothing() {
				yield return null;
			}
			
			static IEnumerator walk(Player player, float amount) {
				return player?.DummyWalkTo(player.X + amount) ?? nothing();
			}
			Register(null, "walk", (player, level, param) => walk(player, GetFloatParam(param, 0, 8)));

			static IEnumerator run(Player player, float time) {
				return player?.DummyRunTo(player.X + time) ?? nothing();
			}
			Register(null, "run", (player, level, param) => run(player, GetFloatParam(param, 0, 8)));

			static IEnumerator look(Player player, Facings direction) {
				player.Facing = direction;
				yield return 0.1f;
			}
			Register(null, "look", (player, level, param) => look(player, GetStringParam(param, 0, "left") == "left" ? Facings.Left : Facings.Right));

			static IEnumerator cameraZoom(Player player, Level level, float zoom, float duration, string easer) {
				player.ForceCameraUpdate = false;
				Ease.Easer ease = GetEaseByName(easer);
				float from = level.Camera.Zoom;
				for(float p = 0f; p < 1f; p += Engine.DeltaTime / duration) {
					level.Camera.Zoom = from + (zoom - from) * ease(p);
					yield return null;
				}

				level.Camera.Zoom = zoom;
			}
			Register(null, "camera_zoom", (player, level, param) => cameraZoom(player, level, GetFloatParam(param, 0, 2), GetFloatParam(param, 1, 2f), GetStringParam(param, 2, "cube")));

			static IEnumerator cameraPanBy(Player p, Level level, Vector2 amount, float time, string easer) {
				p.ForceCameraUpdate = false;
				Vector2 destination = level.Camera.Position + amount;
				return CutsceneEntity.CameraTo(destination, time, GetEaseByName(easer));
			}
			Register(null, "camera_pan", (player, level, param) => cameraPanBy(player, level, new Vector2(GetFloatParam(param, 0), GetFloatParam(param, 1)), GetFloatParam(param, 2, 3), GetStringParam(param, 3, "cube")));

			static IEnumerator cameraPanTo(Player p, Level level, Vector2 destination, float time, string easer) {
				p.ForceCameraUpdate = false;
				return CutsceneEntity.CameraTo(destination, time, GetEaseByName(easer));
			}
			Register(null, "camera_pan_to", (player, level, param) => cameraPanTo(player, level, new Vector2(GetFloatParam(param, 0), GetFloatParam(param, 1)), GetFloatParam(param, 2), GetStringParam(param, 3, "cube")));
			
			static IEnumerator attachCameraToPlayer(Player p) {
				p.ForceCameraUpdate = true;
				yield return null;
			}
			Register(null, "attach_camera_to_player", (player, level, param) => attachCameraToPlayer(player));

			static IEnumerator playerAnimation(Player p, string anim, bool wait) {
				p.DummyAutoAnimate = false;
				if(wait)
					yield return p.Sprite.PlayRoutine(anim);
				else {
					p.Sprite.Play(anim);
					yield return null;
				}
			}
			Register(null, "player_animation", (player, level, param) => playerAnimation(player, GetStringParam(param, 0, "idle"), GetStringParam(param, 1, "start").Equals("play")));

			static IEnumerator playerInventory(Level level, string inventory) {
				var inv = MapMeta.GetInventory(inventory);
				if(inv.HasValue)
					level.Session.Inventory = inv.Value;
				yield return null;
			}
			Register(null, "player_inventory", (player, level, param) => playerInventory(level, GetStringParam(param, 0, "Default")));
			
			
		}

		public static void Unload() {
			ParserHooks.Unload();
		}

		public static void Register(string modName, string triggerName, Func<Player, Level, List<string>, IEnumerator> effect) {
			if(!string.IsNullOrWhiteSpace(modName))
				Triggers.Add(modName.Trim().ToLower() + ":" + triggerName.Trim().ToLower(), effect);
			else
				Triggers.Add(triggerName.Trim().ToLower(), effect);
		}

		public static Func<IEnumerator> Get(string id, Player player, Level level, List<string> p){
			static IEnumerator nothing() {
				yield return null;
			}

			string clean = id?.Trim()?.ToLower() ?? "";
			if(Triggers.ContainsKey(clean)) {
				return () => Triggers[clean](player, level, p);
			} else
				return () => nothing();
		}

		public static float GetFloatParam(List<string> strings, int index, float def = 0) {
			return strings.Count <= index ? def : float.TryParse(strings[index], out float amnt) ? amnt : def;
		}

		public static string GetStringParam(List<string> strings, int index, string def = "") {
			return strings.Count <= index ? def : strings[index];
		}

		public static Ease.Easer GetEaseByName(string name) {
			return name switch {
				"linear" => Ease.Linear,
				"cube" => Ease.CubeInOut,
				"cube_in" => Ease.CubeIn,
				"cube_out" => Ease.CubeOut,
				"quad" => Ease.QuadInOut,
				"quad_in" => Ease.QuadIn,
				"quad_out" => Ease.QuadOut,
				"sine" => Ease.SineInOut,
				"sine_in" => Ease.SineIn,
				"sine_out" => Ease.SineOut,
				"quint" => Ease.QuintInOut,
				"quint_in" => Ease.QuintIn,
				"quint_out" => Ease.QuintOut,
				"exp" => Ease.ExpoInOut,
				"exp_in" => Ease.ExpoIn,
				"exp_out" => Ease.ExpoOut,
				"back" => Ease.BackInOut,
				"back_in" => Ease.BackIn,
				"back_out" => Ease.BackOut,
				"big_back" => Ease.BigBackInOut,
				"big_back_in" => Ease.BigBackIn,
				"big_back_out" => Ease.BigBackOut,
				"elastic" => Ease.ElasticInOut,
				"elastic_in" => Ease.ElasticIn,
				"elastic_out" => Ease.ElasticOut,
				"bounce" => Ease.BounceInOut,
				"bounce_in" => Ease.BounceIn,
				"bounce_out" => Ease.BounceOut,
				_ => Ease.CubeInOut
			};
		}
	}
}
