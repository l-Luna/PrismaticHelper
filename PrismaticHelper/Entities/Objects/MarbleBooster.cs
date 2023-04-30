using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Objects;

[CustomEntity("PrismaticHelper/MarbleBooster")]
public class MarbleBooster : Entity{

	private const string CURRENT_MARBLE_ID = "PrismaticHelper:current_marble_booster";
	private const float GRAVITY = 200f;
	private const float DAMPENING = 0.8f;

	protected Sprite sprite;
	protected int damage = 0;
	
	public MarbleBooster(EntityData data, Vector2 offset) : base(data.Position + offset){
		Depth = Depths.Above;
		Collider = new Circle(10f, y: 2f);
		
		Add(new PlayerCollider(OnPlayer));
		Add(sprite = GFX.SpriteBank.Create("booster"));
	}

	protected void OnPlayer(Player p){
		p.StateMachine.State = PlayerStates.MarbleState;
		SetCurrentMarble(p, this);
		sprite.RemoveSelf();
	}

	public static int MarbleUpdate(Player self){
		if(!self.OnGround())
			self.Speed.Y += GRAVITY * Engine.DeltaTime;

		return PlayerStates.MarbleState;
	}

	public static void MarbleBegin(Player self){
		self.DummyAutoAnimate = false;
	}

	public static void MarbleEnd(Player self){
		self.DummyAutoAnimate = true;
	}

	public static void Load(){
		On.Celeste.Player.OnCollideH += MarbleCollideH;
		On.Celeste.Player.OnCollideV += MarbleCollideV;
	}

	public static void Unload(){
		On.Celeste.Player.OnCollideH -= MarbleCollideH;
		On.Celeste.Player.OnCollideV -= MarbleCollideV;
	}

	private static void MarbleCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data){
		if(self.StateMachine == PlayerStates.MarbleState){
			self.Speed.X *= -DAMPENING;
			return;
		}

		orig(self, data);
	}
	
	private static void MarbleCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data){
		if(self.StateMachine == PlayerStates.MarbleState){
			self.Speed.Y *= -DAMPENING;
			return;
		}

		orig(self, data);
	}

	public static MarbleBooster? CurrentMarble(Player player)
		=> DynamicData.For(player).TryGet<MarbleBooster>(CURRENT_MARBLE_ID, out var marble) ? marble : null;

	public static void SetCurrentMarble(Player player, MarbleBooster marble)
		=> DynamicData.For(player).Set(CURRENT_MARBLE_ID, marble);
}