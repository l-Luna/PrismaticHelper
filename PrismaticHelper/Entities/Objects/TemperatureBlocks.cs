using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Objects;

using CoreMode = Session.CoreModes;

[Tracked]
[CustomEntity("PrismaticHelper/Heater", "PrismaticHelper/Freezer")]
public class TemperatureControlBlock : Solid{
	protected CoreMode Target;
	protected float MaxTime;

	protected CoreMode Previous;
	protected float TimeLeft = 0;
	protected bool Activated = false;
	protected List<Image> HeatImages = new();

	private bool switching = false;

	public bool IsFreezer => Target == CoreMode.Cold;
	public bool IsHeater => Target == CoreMode.Hot;

	public TemperatureControlBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false){
		MaxTime = data.Float("maxTime", 7);
		Target = data.Name == "PrismaticHelper/Heater" ? CoreMode.Hot : CoreMode.Cold;

		OnDashCollide = OnDash;
		Add(new CoreModeListener(OnCoreModeSwitch));

		string sprite = "PrismaticHelper/temperatureBlocks/" + (IsHeater ? "boilerplate" : "freezer");

		// corners
		AddImage(sprite, 0, 0, 0, 0, 8, 8);
		AddImage(sprite, data.Width - 8, 0, 16, 0, 8, 8);
		AddImage(sprite, 0, data.Height - 8, 0, 16, 8, 8);
		AddImage(sprite, data.Width - 8, data.Height - 8, 16, 16, 8, 8);
		for(int i = 1; i < data.Width / 8 - 1; i++){
			// top/bottom row
			AddImage(sprite, i * 8, 0, 8, 0, 8, 8);
			AddImage(sprite, i * 8, data.Height - 8, 8, 16, 8, 8);
		}

		for(int i = 1; i < data.Height / 8 - 1; i++){
			// left/right column
			AddImage(sprite, 0, i * 8, 0, 8, 8, 8);
			AddImage(sprite, data.Width - 8, i * 8, 16, 8, 8, 8);
		}

		for(int i = 1; i < data.Width / 8 - 1; i++){
			for(int j = 1; j < data.Height / 8 - 1; j++){
				// centre
				AddImage(sprite, i * 8, j * 8, 8, 8, 8, 8);
			}
		}

		foreach(var heats in HeatImages){
			heats.Color = Color.Transparent;
		}
	}

	public override void Update(){
		base.Update();
		if(TimeLeft >= 0)
			TimeLeft -= Engine.DeltaTime;
		if(Activated && TimeLeft <= 0){
			TimeLeft = 0;
			Activated = false;
			StartShaking(0.2f);
			var level = SceneAs<Level>();
			// check if any other freezers/heaters are still running instead of inturrupting them,
			var similar = level.Tracker.GetEntities<TemperatureControlBlock>()
				.Cast<TemperatureControlBlock>()
				.Where(x => x != this && x.Target == Target && x.Activated)
				.ToList();
			if(similar.Count == 0)
				level.CoreMode = Previous;
			// remind them what the original core mode was though, if we need to switch
			if(Target != Previous)
				foreach(var block in similar)
					block.Previous = Previous;
		}

		float fraction = TimeLeft / MaxTime;
		var heatColor = Color.White * fraction;
		foreach(var heat in HeatImages){
			heat.Color = heatColor;
		}
	}

	public override void Render(){
		Vector2 realPos = Position;
		Position += Shake;
		base.Render();
		Position = realPos;
	}

	protected DashCollisionResults OnDash(Player player, Vector2 direction){
		bool ignore = false;

		if(!Activated){
			var level = SceneAs<Level>();
			Previous = level.CoreMode;
			// if any temp blocks of the opposite type were activated *and* any wanted to change the core mode back, just turn them off
			// and don't activate
			ignore = level.Tracker.GetEntities<TemperatureControlBlock>()
				.Cast<TemperatureControlBlock>()
				.Any(x => x.Target != Target && x.Activated && x.Previous != x.Target);

			if(Target != Previous){
				switching = true;
				level.CoreMode = Target;
				switching = false;
			}
		}

		if(!ignore){
			TimeLeft = MaxTime;
			Activated = true;
		}

		StartShaking(0.2f);
		return DashCollisionResults.Rebound;
	}

	protected void OnCoreModeSwitch(CoreMode next){
		if(!switching){
			if(Activated)
				StartShaking(0.2f);
			Activated = false;
			TimeLeft = 0;
			// but don't switch back core mode
		}
	}

	protected void AddImage(string sprite, int x, int y, int tx, int ty, int width, int height){
		Add(new Image(GFX.Game[sprite].GetSubtexture(tx, ty, width, height)){
			Position = new(x, y)
		});
		var heat = new Image(GFX.Game[sprite + "_heat"].GetSubtexture(tx, ty, width, height)){
			Position = new(x, y)
		};
		Add(heat);
		HeatImages.Add(heat);
	}
}

[CustomEntity("PrismaticHelper/IceBlock", "PrismaticHelper/SteamBlock")]
public class TemperatureDependentBlock : Solid{
	
	protected CoreMode Required;
	protected Sprite Sprite;

	public bool IsIce => Required == CoreMode.Cold;
	public bool IsSteam => Required == CoreMode.Hot;
	
	public TemperatureDependentBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false){
		Required = data.Name == "PrismaticHelper/SteamBlock" ? CoreMode.Hot : CoreMode.Cold;
		Add(new CoreModeListener(OnCoreModeSwitch));
		Sprite = GFX.SpriteBank.Create("PrismaticHelper_" + (IsIce ? "ice" : "steam"));
		Sprite.Color = Color.White * 0.6f;
		Sprite.Visible = false;
		Add(Sprite);
		AddImages(data.Width, data.Height);
	}

	private void AddImages(int width, int height){
		Random rng = Calc.Random;
		
		// corners
		AddImage(0, 0, 8, 8, 8, 8);
		AddImage(width - 8, 0, 48, 8, 8, 8);
		AddImage(0, height - 8, 8, 48, 8, 8);
		AddImage(width - 8, height - 8, 48, 48, 8, 8);
		for(int i = 1; i < width / 8 - 1; i++){
			// top/bottom row
			AddImage(i * 8, 0, 16 + rng.Next(3) * 8, 8, 8, 8);
			AddImage(i * 8, height - 8, 16 + rng.Next(3) * 8, 48, 8, 8);
		}

		for(int i = 1; i < height / 8 - 1; i++){
			// left/right column
			AddImage(0, i * 8, 8, 16 + rng.Next(3) * 8, 8, 8);
			AddImage(width - 8, i * 8, 48, 16 + rng.Next(3) * 8, 8, 8);
		}

		for(int i = 1; i < width / 8 - 1; i++)
			for(int j = 1; j < height / 8 - 1; j++){
				// centre
				AddImage(i * 8, j * 8, 16 + rng.Next(3) * 8, 16 + rng.Next(3) * 8, 8, 8);
			}
	}

	public override void Added(Scene scene){
		base.Added(scene);

		Collidable = SceneAs<Level>().CoreMode == Required;
		Sprite.Play(Collidable ? "idle" : "blank");
	}

	protected void OnCoreModeSwitch(CoreMode next){
		var wasCollidable = Collidable;
		Collidable = next == Required;
		Level level = SceneAs<Level>();

		if(!Collidable && wasCollidable){
			Vector2 center = Center;
			for(int x = 0; x < Width; x += 4)
				for(int y = 0; y < Height; y += 4){
					Vector2 position = Position + new Vector2(x + 2, y + 2) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
					ParticleType particle = IsIce ? IceBlock.P_Deactivate : FireBarrier.P_Deactivate;
					level.Particles.Emit(particle, position, (position - center).Angle());
				}
			
			Sprite.Play("disappear");
		}

		if(Collidable && !wasCollidable)
			Sprite.Play("appear");
	}
	
	protected void AddImage(int x, int y, int tx, int ty, int width, int height){
		Add(new Subsprite(Sprite, new Rectangle(tx, ty, width, height)){
			Position = new(x, y)
		});
	}
}