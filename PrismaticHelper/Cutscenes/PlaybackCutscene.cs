using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;

namespace PrismaticHelper.Cutscenes;

public static class PlaybackCutscene{
	
	public static IEnumerator Playback(Level l, Player p, string tutorial){
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
	
	// ModInterop exports

	[ModExportName("PrismaticHelper.PlaybackCutscene")]
	public static class ModExports{
		public static IEnumerator PlaybackCutscene(Level l, Player p, string recording){
			return Playback(l, p, recording);
		}
	}
}