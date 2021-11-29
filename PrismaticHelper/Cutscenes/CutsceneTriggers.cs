using System;
using System.Collections;
using System.Collections.Generic;

using Celeste;

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

			static IEnumerator cameraZoom(Player player, Level level, float zoom, float speed) {
				player.ForceCameraUpdate = false;
				while(Math.Abs(level.Camera.Zoom - zoom) > 0) {
					level.Camera.Zoom = Calc.LerpSnap(level.Camera.Zoom, zoom, speed, speed / 5);
					yield return null;
				}
			}
			Register(null, "camera_zoom", (player, level, param) => cameraZoom(player, level, GetFloatParam(param, 0, 2), GetFloatParam(param, 1, 0.05f)));

			static IEnumerator cameraPanBy(Player p, Level level, Vector2 amount) {
				p.ForceCameraUpdate = false;
				Vector2 destination = level.Camera.Position + amount;
				while(Math.Abs((level.Camera.Position - destination).LengthSquared()) > 0.1f * 0.1f) {
					level.Camera.Approach(destination, 0.05f);
					yield return null;
				}
			}
			Register(null, "camera_pan", (player, level, param) => cameraPanBy(player, level, new Vector2(GetFloatParam(param, 0), GetFloatParam(param, 1))));

			static IEnumerator cameraPanTo(Player p, Level level, Vector2 destination) {
				p.ForceCameraUpdate = false;
				while(Math.Abs((level.Camera.Position - destination).LengthSquared()) > 0.1f * 0.1f) {
					level.Camera.Approach(destination, 0.05f);
					yield return null;
				}
			}
			Register(null, "camera_pan_to", (player, level, param) => cameraPanTo(player, level, new Vector2(GetFloatParam(param, 0), GetFloatParam(param, 1))));
			
			static IEnumerator attachCameraToPlayer(Player p) {
				p.ForceCameraUpdate = true;
				yield return null;
			}
			Register(null, "attach_camera_to_player", (player, level, param) => attachCameraToPlayer(player));
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
			return strings.Count < index ? def : float.TryParse(strings[index], out float amnt) ? amnt : def;
		}

		public static string GetStringParam(List<string> strings, int index, string def = "") {
			return strings.Count < index ? def : strings[index];
		}
	}
}
