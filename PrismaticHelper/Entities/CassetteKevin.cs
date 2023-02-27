using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities;

[CustomEntity("PrismaticHelper/CassetteKevin")]
[Tracked]
public class CassetteKevin : CrushBlock{

	private static readonly Color[] colors = {
		Calc.HexToColor("49aaf0"), Calc.HexToColor("f049be"), Calc.HexToColor("fcdc3a"), Calc.HexToColor("38e04e"),
	};
	
	private DashCollision origCollider;
	private Vector2 pendingAttack;
	private Player pendingAttacker;
	private readonly DynamicData myData;
	
	public int index;
	public bool activated;
	
	public CassetteKevin(EntityData data, Vector2 pos) : base(data, pos){
		origCollider = OnDashCollide;
		OnDashCollide = OnDashed;
		myData = new DynamicData(this);

		index = data.Int("index", 0);

		var color = colors[index];
		myData.Set("fill", mul(Calc.HexToColor("363636"), color));
		
		Remove(myData.Get<Sprite>("face"));
		Sprite newFace = GFX.SpriteBank.Create(myData.Get<bool>("giant") ? "PrismaticHelper_giant_crushblock_face" : "PrismaticHelper_crushblock_face");
		Add(newFace);
		newFace.Play("idle");
		newFace.OnLastFrame = f => {
			if(f != "hit")
				return;
			newFace.Play(myData.Get<string>("nextFaceDirection"));
		};
		newFace.Color = color;
		myData.Set("face", newFace);
	}

	private bool CanActivate0(Vector2 direction) => (bool)myData.Invoke("CanActivate", direction);
	
	// note that you do need cassette blocks in the room to actually make these work

	public override void Update(){
		base.Update();
		if(activated){
			if(pendingAttack != Vector2.Zero){
				origCollider(pendingAttacker, pendingAttack);
				pendingAttack = Vector2.Zero; pendingAttacker = null;
			}

			activated = false;
		}
	}

	private DashCollisionResults OnDashed(Player player, Vector2 direction){
		if(!CanActivate0(-direction))
			return DashCollisionResults.NormalCollision;
		pendingAttack = direction;
		pendingAttacker = player;
		return DashCollisionResults.Rebound;
	}

	public static void Load(){
		On.Celeste.CassetteBlockManager.SetActiveIndex += CsActive;
	}

	public static void Unload(){
		On.Celeste.CassetteBlockManager.SetActiveIndex -= CsActive;
	}
	
	private static void CsActive(On.Celeste.CassetteBlockManager.orig_SetActiveIndex orig, CassetteBlockManager self, int i){
		orig(self, i);
		foreach(var entity in self.Scene.Tracker.GetEntities<CassetteKevin>()){
			var kevin = (CassetteKevin)entity;
			kevin.activated |= kevin.index == i; // kevins shouldn't be unactivated, they deactivate themselves appropriately
		}
	}

	private static Color mul(Color l, Color r){
		return new Color((l.R / 255f) * (r.R / 255f), (l.G / 255f) * (r.G / 255f), (l.B / 255f) * (r.B / 255f), (l.A / 255f) * (r.A / 255f));
	}
}