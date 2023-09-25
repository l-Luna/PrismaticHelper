using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Objects;

[CustomEntity("PrismaticHelper/CassetteBerry")]
public class CassetteBerry : Strawberry{

	protected int Index = 0;
	protected bool Distracted = false;
	
	protected bool Collectable = true;
	protected bool IsGhost = false;

	protected Sprite Sprite, Headphones;
	
	public CassetteBerry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid){
		Index = data.Int("index");
		Distracted = data.Bool("distracted");
		
		Add(new CassetteListener{
			OnBeat = idx => {
				Collectable = idx == Index;
				Sprite spr = Get<Sprite>();
				if(Collectable){
					if(spr.CurrentAnimationID == "deactive") spr.Play("idle");
					if(spr.CurrentAnimationID == "flap_deactive"){
						spr.Play("flap");
						Headphones?.Play("idle", restart: true);
					}
				}else{
					if(spr.CurrentAnimationID == "idle") spr.Play("deactive");
					if(spr.CurrentAnimationID == "flap"){
						spr.Play("flap_deactive");
						Headphones?.Play("idle", restart: true);
					}
				}

				var bloom = Get<BloomPoint>();
				if(bloom != null)
					bloom.Alpha = Golden || Moon || IsGhost || !Collectable ? .5f : 1;
			}
		});
		
		PlayerCollider pc = Get<PlayerCollider>();
		if(pc != null){
			var oldCollide = pc.OnCollide;
			pc.OnCollide = p => {
				if(Collectable){
					oldCollide(p);
					Headphones?.RemoveSelf();
				}
			};
		}

		if(Distracted && Winged){
			DashListener dl = Get<DashListener>();
			if(dl != null && Distracted){
				var oldDash = dl.OnDash;
				dl.OnDash = dir => {
					if(Collectable)
						oldDash(dir);
				};
			}
		}
	}
	
	public override void Added(Scene scene){
		base.Added(scene);
		IsGhost = SaveData.Instance.CheckStrawberry(ID);
		Sprite = Get<Sprite>();
		GFX.SpriteBank.CreateOn(Sprite, "PrismaticHelper_cas_" + (IsGhost ? "ghost" : "") + "berry_" + Index switch{
			0 => "blue",
			1 => "rose",
			2 => "sun",
			3 => "malachite",
			_ => "blue"
		});
		if(Winged){
			Sprite.Play("flap");
			if(Distracted)
				Add(Headphones = GFX.SpriteBank.Create("PrismaticHelper_cas_headphones"));
		}
		var oldFrameChange = Sprite.OnFrameChange;
		Sprite.OnFrameChange = id => oldFrameChange(id == "flap_deactive" ? "flap" : id);
	}

	public override void Update(){
		base.Update();

		if(Headphones != null){
			Headphones.Rotation = Sprite.Rotation;
			Headphones.Scale = Sprite.Scale;
			Headphones.Rate = Sprite.Rate;
		}
	}
}