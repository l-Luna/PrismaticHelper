using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Objects;

// Up to 10% less jank than competing brands!
[CustomEntity("PrismaticHelper/SpeedBerry")]
[RegisterStrawberry(false, false)]
public class SpeedBerry : Strawberry{
	
	private SpeedBerryPopupCountdown countdown;
	private DynamicData componentsData, selfData;
	private float wobble;
	public int seconds;
	
	public SpeedBerry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid){
		seconds = data.Int("time");

		componentsData = new DynamicData(Components);
		selfData = new DynamicData(this);
		ReturnHomeWhenLost = false;

		// don't visually return on death
		PlayerCollider collider = Get<PlayerCollider>();
		Action<Player> onPlayer = collider.OnCollide;
		collider.OnCollide = player => {
			onPlayer(player);
			ReturnHomeWhenLost = false;
		};
	}

	public override void Added(Scene scene){
		base.Added(scene);
		GFX.SpriteBank.CreateOn(Get<Sprite>(), "PrismaticHelper_speed_berry");
	}

	public override void Awake(Scene scene){
		base.Awake(scene);
		scene.Add(countdown = new SpeedBerryPopupCountdown{
			entity = this
		});
	}

	public override void Update(){
		//base.Update();
		// skip Strawberry.Update
		//Components.Update();
		componentsData.Invoke("Update");

		if(!selfData.Get<bool>("collected")){
			var chapterTime = TimeSpan.FromTicks(SceneAs<Level>().Session.Time);
			var left = TimeSpan.FromSeconds(seconds) - chapterTime;
			if(left.Ticks <= 0)
				TimeUp();
		}

		// and then normal code
		wobble += Engine.DeltaTime * 4f;
		countdown.Y = Get<Sprite>().Y = Get<BloomPoint>().Y = Get<VertexLight>().Y = (float) Math.Sin(wobble) * 2;
		
		if(Follower.Leader != null && Scene.OnInterval(0.08f)){
			ParticleType type = P_Glow;
			SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
		}
	}
	
	public bool Grabbed => Follower.HasLeader;

	protected void TimeUp(){
		RemoveSelf();
		SceneAs<Level>().Add(new SpeedBerryExplosion{
			Position = Position
		});
		// don't use LoseFollower since we don't want the return animation
		if(Follower.Leader is Leader leader)
			leader.Followers.Remove(Follower);
	}

	public static Vector2 WorldToScreen(Vector2 v, Level l){
		v -= l.Camera.Position;
		if(SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
			v.X = 320 - v.X;
		v.X *= 6;
		v.Y *= 6;
		return v;
	}

	public class SpeedBerryPopupCountdown : Entity{

		internal SpeedBerry entity;
		protected float fadeOut = 1;
		
		public SpeedBerryPopupCountdown(){
			Tag |= TagsExt.SubHUD;
		}

		public override void Render(){
			base.Render();
			if(Scene is not Level l)
				return;
			if(fadeOut <= 0)
				RemoveSelf();

			var chapterTime = TimeSpan.FromTicks(l.Session.Time);
			var left = TimeSpan.FromSeconds(entity.seconds) - chapterTime;

			var text = $"{left:mm:ss.ffff}";
			if(left.Ticks < 0)
				text = "-" + text;

			ActiveFont.DrawOutline(text, WorldToScreen(entity.Position + Position, l), new(0.5f, 0), Vector2.One * 0.7f, Color.White * fadeOut, 2, Color.Black * fadeOut);

			if(entity.Grabbed || left.Ticks < 0)
				fadeOut = MathHelper.Lerp(fadeOut, 0, 0.1f);
		}
	}

	public class SpeedBerryExplosion : Entity{

		protected float fadeOut = 1;
		protected bool startFade = false;
		protected Sprite sprite;
		
		public SpeedBerryExplosion(){
			sprite = GFX.SpriteBank.Create("PrismaticHelper_speed_berry");
			sprite.Play("explode");
			sprite.OnFinish = _ => startFade = true;
			Add(sprite);
		}

		public override void Update(){
			base.Update();

			if(startFade)
				fadeOut = MathHelper.Lerp(fadeOut, 0, 0.1f);
			if(fadeOut <= 0)
				RemoveSelf();
			sprite.Color = Color.White * fadeOut;
		}
	}
}