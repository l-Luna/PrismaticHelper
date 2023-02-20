using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Cutscenes;

public static class CutsceneTriggers{
	
	public static readonly Dictionary<string, Func<Player, Level, List<string>, IEnumerator>> Triggers = new();
	private static BadelineDummy baddy; // :3

	public static void Load(){
		
		ParserHooks.LoadHooks();

		static IEnumerator nothing(){
			yield return null;
		}

		static IEnumerator walk(Player player, float amount){
			return player?.DummyWalkTo(amount) ?? nothing();
		}

		Register("walk", (player, level, param) => walk(player, player.X + GetFloatParam(param, 0, 8)));
		Register("walk_to", (player, level, param) => walk(player, GetFloatParam(param, 0, 8)));

		static IEnumerator run(Player player, float time){
			return player?.DummyRunTo(time) ?? nothing();
		}

		Register("run", (player, level, param) => run(player, player.X + GetFloatParam(param, 0, 8)));
		Register("run_to", (player, level, param) => run(player, GetFloatParam(param, 0, 8)));

		static IEnumerator look(Player player, Facings direction){
			player.Facing = direction;
			yield return 0.1f;
		}

		Register("look", (player, level, param) => look(player, GetStringParam(param, 0, "left") == "left" ? Facings.Left : Facings.Right));

		static IEnumerator @goto(Player player, float x, float y){
			player.Position.X = x;
			player.Position.Y = y;
			yield return null;
		}

		Register("goto", (player, level, param) => @goto(player, GetFloatParam(param, 0, 0), GetFloatParam(param, 1, 0)));

		static IEnumerator cameraZoomBack(Level l, float duration){
			return l.ZoomBack(duration);
		}

		Register("camera_zoom_back", (player, level, param) => cameraZoomBack(level, GetFloatParam(param, 0, 1)));

		static IEnumerator cameraZoom(Player player, Level level, float zoom, float duration, string easer){
			player.ForceCameraUpdate = false;
			Ease.Easer ease = GetEaseByName(easer);
			float from = level.Camera.Zoom;
			for(float p = 0f; p < 1f; p += Engine.DeltaTime / duration){
				level.Camera.Zoom = from + (zoom - from) * ease(p);
				yield return null;
			}

			level.Camera.Zoom = zoom;
		}

		Register("camera_zoom", (player, level, param) => cameraZoom(player, level, GetFloatParam(param, 0, 2), GetFloatParam(param, 1, 2f), GetStringParam(param, 2, "cube")));

		static IEnumerator cameraPanBy(Player p, Level level, Vector2 amount, float time, string easer){
			p.ForceCameraUpdate = false;
			Vector2 destination = level.Camera.Position + amount;
			return CutsceneEntity.CameraTo(destination, time, GetEaseByName(easer));
		}

		Register("camera_pan", (player, level, param) => cameraPanBy(player, level, new Vector2(GetFloatParam(param, 0), GetFloatParam(param, 1)), GetFloatParam(param, 2, 3), GetStringParam(param, 3, "cube")));

		static IEnumerator cameraPanTo(Player p, Level level, Vector2 destination, float time, string easer){
			p.ForceCameraUpdate = false;
			return CutsceneEntity.CameraTo(destination, time, GetEaseByName(easer));
		}

		Register("camera_pan_to", (player, level, param) => cameraPanTo(player, level, new Vector2(GetFloatParam(param, 0), GetFloatParam(param, 1)), GetFloatParam(param, 2), GetStringParam(param, 3, "cube")));

		static IEnumerator attachCameraToPlayer(Player p){
			p.ForceCameraUpdate = true;
			yield return null;
		}

		Register("attach_camera_to_player", (player, level, param) => attachCameraToPlayer(player));

		static IEnumerator playerAnimation(Player p, string anim, bool wait){
			p.DummyAutoAnimate = false;
			if(wait)
				yield return p.Sprite.PlayRoutine(anim);
			else{
				p.Sprite.Play(anim);
				yield return null;
			}
		}

		Register("player_animation", (player, level, param) => playerAnimation(player, GetStringParam(param, 0, "idle"), GetStringParam(param, 1, "start").Equals("play")));

		static IEnumerator playerInventory(Level level, string inventory){
			var inv = MapMeta.GetInventory(inventory);
			if(inv.HasValue)
				level.Session.Inventory = inv.Value;
			yield return null;
		}

		Register("player_inventory", (player, level, param) => playerInventory(level, GetStringParam(param, 0, "Default")));

		static IEnumerator waitForGround(Player p){
			while(!p.OnGround())
				yield return null;
		}

		Register("wait_for_ground", (player, level, param) => waitForGround(player));

		static IEnumerator hideEntitiesByName(Level l, string entityName){
			l.Entities
				.Where(k => k.GetType().Name.Equals(entityName))
				.ToList()
				.ForEach(k => k.Visible = false);
			yield return null;
		}

		Register("hide_entities", (player, level, param) => hideEntitiesByName(level, GetStringParam(param, 0)));

		static IEnumerator showNextBooster(Level l){
			l.Entities.FindAll<Booster>()
				.FirstOrDefault(k => !k.Visible)?
				.Appear();
			yield return null;
		}

		Register("show_next_booster", (player, level, param) => showNextBooster(level));

		static IEnumerator showNextDoor(Level l, int soundIdx){
			var d = l.Entities.FindAll<LockBlock>()
				.FirstOrDefault(k => !k.Visible);
			if(d != null){
				d.Appear();
				Audio.Play("event:/new_content/game/10_farewell/locked_door_appear_" + soundIdx, d.Center);
			}

			yield return null;
		}

		Register("show_next_door", (player, level, param) => showNextDoor(level, (int)GetFloatParam(param, 0, 1)));

		// baddy controls

		static IEnumerator baddyAppear(Level l, Player p, float xOffset, float yOffset){
			if(baddy != null && baddy.Scene == Engine.Scene)
				baddy.Vanish();
			baddy = new BadelineDummy(p.Center + new Vector2(xOffset, yOffset));
			l.Add(baddy);
			baddy.Appear(l);
			yield return null;
		}

		Register("baddy_appear", (player, level, param) => baddyAppear(level, player, GetFloatParam(param, 0, 0), GetFloatParam(param, 1, 0)));

		static IEnumerator baddySplit(Level l, Player p, float xOffset, float yOffset, bool facePlayer){
			if(baddy != null && baddy.Scene == Engine.Scene)
				baddy.Vanish();
			baddy = new BadelineDummy(p.Center);
			l.Add(baddy);
			p.CreateSplitParticles();
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			l.Displacement.AddBurst(p.Center, 0.4f, 8f, 32f, 0.5f);
			Audio.Play("event:/char/badeline/maddy_split", p.Position);
			Vector2 target = p.Center + new Vector2(xOffset, yOffset);
			baddy.Sprite.Scale.X = Math.Sign(target.X - p.X) * (facePlayer ? -1 : 1);
			return baddy.FloatTo(target, 1, faceDirection: false);
		}

		Register("baddy_split", (player, level, param) => baddySplit(level, player, GetFloatParam(param, 0, 0), GetFloatParam(param, 1, 0), GetStringParam(param, 2, "true").Equals("true")));

		static IEnumerator baddyFloatTo(float x, float y, bool look){
			if(baddy == null)
				return nothing();
			return baddy.FloatTo(new Vector2(x, y), faceDirection: look);
		}

		Register("baddy_float_to", (player, level, param) => baddyFloatTo(GetFloatParam(param, 0, 0), GetFloatParam(param, 1, 0), GetStringParam(param, 2, "true").Equals("true")));
		Register("baddy_float_by", (player, level, param) => baddyFloatTo(GetFloatParam(param, 0, 0) + baddy.X, GetFloatParam(param, 1, 0) + baddy.Y, GetStringParam(param, 2, "true").Equals("true")));
		Register("baddy_float_by_player", (player, level, param) => baddyFloatTo(GetFloatParam(param, 0, 0) + player.X, GetFloatParam(param, 1, 0) + player.Y, GetStringParam(param, 2, "true").Equals("true")));

		static IEnumerator baddyLook(bool left){
			if(baddy == null)
				yield break;
			baddy.Sprite.X = left ? -1 : 1;
			yield return null;
		}

		Register("baddy_look", (player, level, param) => baddyLook(GetStringParam(param, 0, "left").Equals("left")));

		static IEnumerator baddyCombine(Level l, Player pl){
			if(baddy == null)
				yield break;
			Vector2 from = baddy.Position;
			for(float p = 0f; p < 1f; p += Engine.DeltaTime / 0.25f){
				baddy.Position = Vector2.Lerp(from, pl.Position, Ease.CubeIn(p));
				yield return null;
			}

			baddy.Visible = false;
			l.Displacement.AddBurst(pl.Position, 0.4f, 8f, 32f, 0.5f);
		}

		Register("baddy_combine", (player, level, param) => baddyCombine(level, player));

		static IEnumerator baddyVanish(){
			baddy.Vanish();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			baddy = null;
			yield return null;
		}

		Register("baddy_vanish", (player, level, param) => baddyVanish());

		static IEnumerator baddyAnimation(string anim, bool wait){
			if(baddy == null)
				yield break;
			if(wait)
				yield return baddy.Sprite.PlayRoutine(anim);
			else{
				baddy.Sprite.Play(anim);
				yield return null;
			}
		}

		Register("baddy_animation", (player, level, param) => baddyAnimation(GetStringParam(param, 0, "idle"), GetStringParam(param, 1, "start").Equals("play")));

		// level controls
		
		static IEnumerator setFlag(Level l, string name, bool value){
			l.Session.SetFlag(name, value);
			yield return null;
		}
		
		Register("set_flag", (player, level, param) => setFlag(level, GetStringParam(param, 0), !GetStringParam(param, 1, "true").Equals("false")));
		
		// player playback

		static IEnumerator playback(Level l, Player p, string tutorial){
			List<Player.ChaserState> playback = PlaybackData.Tutorials[tutorial];
			float time = 0;
			int idx = 0;
			Vector2 initial = p.Position;
			float lastDashStart = 0, dashTrailTimer = 0;
			while(idx < playback.Count){
				time += Engine.DeltaTime;
				if(time >= playback[idx].TimeStamp){
					idx++;
					if(idx == playback.Count)
						break;
					var state = playback[idx];
					var next = playback.Count > idx + 1 ? playback[idx + 1] : state;
					p.Position = initial + state.Position;
					p.Speed = Vector2.Zero;
					p.Facing = state.Facing;
					p.Sprite.Scale = state.Scale.Abs();
					if(p.Sprite.CurrentAnimationID != state.Animation && state.Animation != null && p.Sprite.Has(state.Animation)){
						p.Sprite.Play(state.Animation, true);
						p.DashDir = next.DashDirection;
						
						// this is how vanilla PlayerPlayback does it /shrug
						if(state.Animation.Equals("dash") && time - lastDashStart >= 0.15){
							p.Play(state.Scale.X > 0 ? "event:/char/madeline/dash_red_right" : "event:/char/madeline/dash_red_left");
							Celeste.Celeste.Freeze(0.05f);
							Dust.Burst(p.Position, (-p.DashDir).Angle(), 8, Player.P_DashA);
							Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
							l.Displacement.AddBurst(p.Center, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
							l.DirectionalShake(p.DashDir, 0.2f);
							SlashFx.Burst(p.Center, p.DashDir.Angle());
							lastDashStart = time;
						}

						foreach(var component in l.Tracker.GetComponents<DashListener>().Cast<DashListener>())
							component.OnDash?.Invoke(p.DashDir);
					}
					p.OverrideHairColor = state.HairColor;
				}

				if(p.Sprite.CurrentAnimationID.Equals("dash")){
					if(dashTrailTimer <= 0){
						dashTrailTimer = 0.1f;
						// TODO: would prefer to use wasDashB and CreateTrail, but those, don't work
						// TODO: support player as badeline, hair mods?
						TrailManager.Add(p, new Vector2(Math.Abs(p.Sprite.Scale.X) * (float)p.Facing, p.Sprite.Scale.Y), p.Dashes == 2 ? Player.NormalHairColor : Player.UsedHairColor);
					}

					if(l.OnInterval(0.02f))
						l.ParticlesFG.Emit(p.Dashes == 2 ? p.Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline ? Player.P_DashB : Player.P_DashBadB : Player.P_DashA, p.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), p.DashDir.Angle());
				}

				if(dashTrailTimer >= 0)
					dashTrailTimer -= Engine.DeltaTime;

				yield return null;
			}

			p.OverrideHairColor = null;
			p.ForceCameraUpdate = true;
			yield return null;
		}
		
		Register("run_playback", (player, level, param) => playback(level, player, GetStringParam(param, 0)));
	}

	public static void Unload(){
		ParserHooks.Unload();
	}

	internal static void CleanupOnSkip(Level l, Player p){
		baddy?.RemoveSelf();
		p.OverrideHairColor = null;
	}
	
	private static void Register(string triggerName, Func<Player, Level, List<string>, IEnumerator> effect){
		Register(null, triggerName, effect);
	}

	public static void Register(string modName, string triggerName, Func<Player, Level, List<string>, IEnumerator> effect){
		if(!string.IsNullOrWhiteSpace(modName))
			Triggers.Add(modName.Trim().ToLower() + ":" + triggerName.Trim().ToLower(), effect);
		else
			Triggers.Add(triggerName.Trim().ToLower(), effect);
	}

	public static Func<IEnumerator> Get(string id, Player player, Level level, List<string> p){
		static IEnumerator nothing(){
			yield return null;
		}

		string clean = id?.Trim()?.ToLower() ?? "";
		if(Triggers.ContainsKey(clean))
			return () => Triggers[clean](player, level, p);
		return nothing;
	}

	public static float GetFloatParam(List<string> strings, int index, float def = 0){
		return strings.Count <= index ? def : float.TryParse(strings[index], out float amnt) ? amnt : def;
	}

	public static string GetStringParam(List<string> strings, int index, string def = ""){
		return strings.Count <= index ? def : strings[index];
	}

	public static Ease.Easer GetEaseByName(string name){
		return name switch{
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