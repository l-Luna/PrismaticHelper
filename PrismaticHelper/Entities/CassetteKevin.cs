using System.Collections.Generic;
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
		myData.Set("face", newFace);

		// easier to just readd borders
		foreach(var c in Components.ToArray())
			if(c is Image i && i != newFace)
				Remove(i);
		
		List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("PrismaticHelper/cassetteKevin/block");
		MTexture idle;
		switch(data.Attr("axes")){
			case "both":
				idle = atlasSubtextures[3]; break;
			case "horizontal":
				idle = atlasSubtextures[1]; break;
			case "vertical":
				idle = atlasSubtextures[2]; break;
			default:
				idle = atlasSubtextures[0]; break;
		}
		
		var x1 = (int) (Width / 8.0) - 1;
		var y1 = (int) (Height / 8.0) - 1;
		AddImage0(idle, 0, 0, 0, 0, -1, -1);
		AddImage0(idle, x1, 0, 3, 0, 1, -1);
		AddImage0(idle, 0, y1, 0, 3, -1, 1);
		AddImage0(idle, x1, y1, 3, 3, 1, 1);
		for(int x2 = 1; x2 < x1; ++x2){
			AddImage0(idle, x2, 0, Calc.Random.Choose(1, 2), 0, borderY: -1);
			AddImage0(idle, x2, y1, Calc.Random.Choose(1, 2), 3, borderY: 1);
		}
		for(int y2 = 1; y2 < y1; ++y2){
			AddImage0(idle, 0, y2, 0, Calc.Random.Choose(1, 2), -1);
			AddImage0(idle, x1, y2, 3, Calc.Random.Choose(1, 2), 1);
		}
		
		foreach(var c in Components)
			if(c is Image i)
				i.Color = color;
	}

	private bool CanActivate0(Vector2 direction) => (bool)myData.Invoke("CanActivate", direction);
	private void ActivateParticles0(Vector2 direction) => myData.Invoke("ActivateParticles", direction);
	private void AddImage0(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0) => myData.Invoke("AddImage", idle, x, y, tx, ty, borderX, borderY);
	
	// note that you do need cassette blocks in the room to actually make these work

	public override void Update(){
		base.Update();
		if(activated){
			if(pendingAttack != Vector2.Zero){
				IndicateLitSides(Vector2.Zero);
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
		IndicateLitSides(-direction);
		ActivateParticles0(-direction);
		Audio.Play("event:/game/06_reflection/crushblock_rest_waypoint", Center);
		return DashCollisionResults.Rebound;
	}

	private void IndicateLitSides(Vector2 hitSide){
		IndicateSide(hitSide.X < 0, myData.Get<List<Image>>("activeLeftImages"));
		IndicateSide(hitSide.X > 0, myData.Get<List<Image>>("activeRightImages"));
		IndicateSide(hitSide.Y < 0, myData.Get<List<Image>>("activeTopImages"));
		IndicateSide(hitSide.Y > 0, myData.Get<List<Image>>("activeBottomImages"));
	}

	private void IndicateSide(bool on, List<Image> sideImgs){
		var color = colors[index];
		foreach(var img in sideImgs){
			if(on){
				img.Visible = true;
				img.Color = darken(color);
			}else{
				img.Visible = false;
				img.Color = color;
			}
		}
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

	private static Color darken(Color c){
		return new Color(c.R * (0.6f/255f), c.G * (0.6f/255f), c.B * (0.6f/255f), 1);
	}
}