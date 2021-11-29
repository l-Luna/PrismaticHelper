using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Celeste;
using Celeste.Mod.Entities;

using Microsoft.Xna.Framework;

using Monocle;

namespace PrismaticHelper.Entities {

	[CustomEntity("PrismaticHelper/MultiLockedDoor")]
	internal class MultiLockedDoor : Solid {

		public static ParticleType P_LockBurst = new(Key.P_Collect) {
			Color = Calc.HexToColor("9badb7"),
			Color2 = Calc.HexToColor("9badb7")
		};

		public static string DefaultDoor = "PrismaticHelper/multiLockDoor/base_wood";
		public static string DefaultLock = "PrismaticHelper/multiLockDoor/mini_lock";
		public static string DefaultUnlockSfx = "event:/game/03_resort/key_unlock";

		public EntityID ID;
		public string Door, Lock;
		public string UnlockSfx;

		public int KeysRequired = 1;
		public bool Opening = false;

		private float lockFrame = 0, doorFrame =0;

		public MultiLockedDoor(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, 32, 32, false) {
			ID = id;
			Door = data.Attr("door");
			if(string.IsNullOrWhiteSpace(Door))
				Door = DefaultDoor;

			Lock = data.Attr("lock");
			if(string.IsNullOrWhiteSpace(Lock))
				Lock = DefaultLock;

			UnlockSfx = data.Attr("unlockSfx");
			if(string.IsNullOrWhiteSpace(UnlockSfx))
				UnlockSfx = DefaultUnlockSfx;
			UnlockSfx = SFX.EventnameByHandle(UnlockSfx);

			KeysRequired = (int)MathHelper.Max(data.Int("keys", 1), 1);

			DisableLightsInside = false;

			Add(new PlayerCollider(CollectKeys, new Circle(60f, 16f, 16f)));
		}

		private void CollectKeys(Player player) {
			if(Opening)
				return;

			int keysCollected = player.Leader.Followers.Where(k => k.Entity is Key key && !key.StartedUsing).Count();
			if(keysCollected >= KeysRequired)
				TryOpen(player);
		}

		private void TryOpen(Player player) {
			Collidable = false;
			if(!Scene.CollideCheck<Solid>(player.Center, Center)) {
				Opening = true;
				List<Key> inserting = new();
				foreach(var key in player.Leader.Followers.Where(k => k.Entity is Key key && !key.StartedUsing).Select(k => (Key)k.Entity)) {
					if(inserting.Count < KeysRequired) {
						key.StartedUsing = true;
						Add(new Coroutine(key.UseRoutine(GetLockCenter(inserting.Count))));
						inserting.Add(key);
					}
				}

				Add(new Coroutine(UnlockRoutine(inserting)));
			}
			Collidable = true;
		}

		private IEnumerator UnlockRoutine(List<Key> inserting) {
			Level level = SceneAs<Level>();

			SoundEmitter emitter = SoundEmitter.Play(UnlockSfx, this);
			emitter.Source.DisposeOnTransition = true;
			yield return 1.2f;

			level.Session.DoNotLoad.Add(ID);
			inserting.ForEach(k => k.RegisterUsed());
			bool turning = true;
			while(turning) {
				turning = false;
				for(int i = 0; i < inserting.Count; i++) {
					Key key = inserting[i];
					if(key.Turning)
						turning = true;
				}
				yield return null;
			}

			Tag |= Tags.TransitionUpdate;
			Collidable = false;
			emitter.Source.DisposeOnTransition = false;

			while(lockFrame < 6) {
				lockFrame = Math.Min(lockFrame + 1/5f, 6);
				yield return null;
			}

			for(int i = 0; i < KeysRequired; i++)
				level.ParticlesFG.Emit(P_LockBurst, 13, GetLockCenter(i), Vector2.One / 3);	

			level.Shake();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			while(doorFrame < 6) {
				doorFrame = Math.Min(doorFrame + 1 / 5f, 6);
				yield return null;
			}

			RemoveSelf();
		}

		public override void Render() {
			base.Render();
			GFX.Game[Door].Draw(Position, Vector2.Zero, Color.White * (1 - (doorFrame / 6)));
			if(KeysRequired == 1) {
				GFX.Game[Lock + (int)lockFrame].DrawCentered(GetLockCenter(0));
			} else
				for(int i = 0; i < KeysRequired; i++)
					GFX.Game[Lock + (int)lockFrame].DrawCentered(GetLockCenter(i));
		}

		public Vector2 GetLockCenter(int lockIndex) {
			var center = Position + new Vector2(GFX.Game[Door].Width / 2, GFX.Game[Door].Height / 2); // Center doesn't acommodate larger textures, this does
			if(KeysRequired == 1)
				return center;
			return center + new Vector2((float)(10 * Math.Cos(2 * lockIndex * Math.PI / KeysRequired)), (float)(10 * Math.Sin(2 * lockIndex * Math.PI / KeysRequired)));
		}
	}
}
