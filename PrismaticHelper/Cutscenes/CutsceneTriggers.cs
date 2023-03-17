using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using PrismaticHelper.Entities.Cutscenes;
using PrismaticHelper.Entities.Panels;

namespace PrismaticHelper.Cutscenes;

public static class CutsceneTriggers{
	
	public static readonly Dictionary<string, Func<Player, Level, List<string>, IEnumerator>> Triggers = new();
	public static readonly List<Action<Level, List<Scriptable>>> ScriptableProviders = new();

	// NOT looking forward to making these customisable
	private static readonly ParticleType scratch = new ParticleType()
	{
		Color = Calc.HexToColor("3C7913"),
		Color2 = Calc.HexToColor("D2D2D2"),
		ColorMode = ParticleType.ColorModes.Blink,
		FadeMode = ParticleType.FadeModes.Late,
		Size = 1f,
		Direction = 0.0f,
		DirectionRange = 6.2831855f,
		SpeedMin = 5f,
		SpeedMax = 10f,
		LifeMin = 0.6f,
		LifeMax = 1.2f,
		SpeedMultiplier = 0.3f
	};
	
	// TODO: scriptable baddy, player
	private static BadelineDummy baddy; // :3

	public static void Load(){
		
		ParserHooks.LoadHooks();
		
		typeof(ModExports).ModInterop();
		typeof(PlaybackCutscene.ModExports).ModInterop();

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

		static IEnumerator gotoRoom(Player player, Level l, string roomName, string intro){
			l.EndCutscene();
			l.OnEndOfFrame += () => {
				l.TeleportTo(player, roomName, GetIntroByName(intro));
				l.Wipe?.Cancel();
			};
			DynamicData.For(l).Set("PrismaticHelper:force_unskippable", false);
			yield return null;
		}
		
		Register("goto_room", (player, level, param) => gotoRoom(player, level, GetStringParam(param, 0), GetStringParam(param, 1, "None")));

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
			p.DummyAutoAnimate = true;
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

		static IEnumerator disableSkip(Level l){
			DynamicData.For(l).Set("PrismaticHelper:force_unskippable", true);
			yield return null;
		}
		
		Register("disable_skip", (player, level, param) => disableSkip(level));

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

		Register("run_playback", (player, level, param) => PlaybackCutscene.Playback(level, player, GetStringParam(param, 0)));
		
		// visual effects

		static IEnumerator glitchEffect(float duration){
			Glitch.Value = 0.3f;
			yield return duration;
			Glitch.Value = 0;
		}
		
		Register("glitch_effect", (player, level, param) => glitchEffect(GetFloatParam(param, 0, 0.5f)));
		
		// TODO: WIP controls

		static IEnumerator introduce(Player p, Level l, string name, string sprite, float offX, float offY){
			l.Add(new Puppet(name, sprite){
				Position = p.Position + new Vector2(offX, offY)
			});
			yield return null;
		}
		
		Register("tmp", "introduce", (player, level, param) => introduce(player, level, GetStringParam(param, 0, null), GetStringParam(param, 1, null), GetFloatParam(param, 2), GetFloatParam(param, 3)));

		static IEnumerator scare(Level l, string puppetName){
			var puppet = l.Tracker.GetEntities<Puppet>().Cast<Puppet>().FirstOrDefault(x => x.ScName() == puppetName);
			if(puppet != null){
				var point = puppet.Center + puppet.ScSprite().Center;
				Audio.Play("event:/char/badeline/appear", point);
				l.Displacement.AddBurst(point, 0.5f, 24f, 96f, 0.4f);
				l.Particles.Emit(scratch, 30, point, Vector2.One * 12f);
			}
			yield return null;
		}
		
		Register("tmp", "scare", (player, level, param) => scare(level, GetStringParam(param, 0, null)));

		static IEnumerator spt(Player p, Level l, string targetName, string puppetName, string maskName, float duration, string introType){
			var puppet = l.Tracker.GetEntities<Puppet>().Cast<Puppet>().FirstOrDefault(x => x.ScName() == puppetName);
			if(puppet != null){
				AbstractPanel panel = new Windowpane(new EntityData(), Vector2.Zero){
					Position = puppet.Position + puppet.ScSprite().Center,
					Opacity = 1,
					RoomName = targetName,
					Mask = maskName,
					Foreground = true
				};
				l.Add(panel);
				yield return null;
				for(float progress = 0.0f; progress < 1.0; progress += Engine.DeltaTime / duration){
					panel.Scale = 1 + Ease.CubeIn(progress) * 80;
					yield return null;
				}
				panel.Opacity = 1;
				yield return null;
				yield return gotoRoom(p, l, targetName, introType);
			}else
				yield return gotoRoom(p, l, targetName, introType);
		}
		
		Register("tmp", "spt", (player, level, param) => spt(player, level, GetStringParam(param, 0, null), GetStringParam(param, 1, null), GetStringParam(param, 2), GetFloatParam(param, 3), GetStringParam(param, 4)));
	}

	public static void Unload(){
		ParserHooks.Unload();
	}

	internal static void CleanupOnSkip(Level l, Player p){
		// TODO: only run if there was actually any PH triggers
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

	private static void RegisterScriptProvider(Action<Level, List<Scriptable>> provider){
		ScriptableProviders.Add(provider);
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

	public static Player.IntroTypes GetIntroByName(string name){
		return Enum.TryParse(name, true, out Player.IntroTypes type) ? type : Player.IntroTypes.None;
	}
	
	// ModInterop exports

	[ModExportName("PrismaticHelper.CutsceneTriggers")]
	public static class ModExports{
		public static void RegisterTrigger(string modName, string triggerName, Func<Player, Level, List<string>, IEnumerator> effect){
			Register(modName, triggerName, effect);
		}
	}
}