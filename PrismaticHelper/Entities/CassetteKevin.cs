using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities;

[CustomEntity("PrismaticHelper/CassetteKevin")]
[Tracked]
public class CassetteKevin : CrushBlock{

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
}