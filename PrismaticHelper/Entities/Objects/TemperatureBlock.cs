using System.Linq;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Objects;

using CoreMode = Session.CoreModes;

[Tracked]
[CustomEntity("PrismaticHelper/Heater", "PrismaticHelper/Freezer")]
public class TemperatureBlock : Solid{
	
	protected CoreMode Target;
	protected float MaxTime;

	protected CoreMode Previous;
	protected float TimeLeft = 0;
	protected bool Activated = false;

	private bool switching = false;

	public bool IsFreezer => Target == CoreMode.Cold;
	public bool IsHeater => Target == CoreMode.Hot;

	public TemperatureBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false){
		MaxTime = data.Float("maxTime", 7);
		Target = data.Name == "PrismaticHelper/Heater" ? CoreMode.Hot : CoreMode.Cold;

		OnDashCollide = OnDash;
		Add(new CoreModeListener(OnCoreModeSwitch));
	}

	public override void Update(){
		base.Update();
		if(TimeLeft >= 0)
			TimeLeft -= Engine.DeltaTime;
		if(Activated && TimeLeft <= 0){
			TimeLeft = 0;
			Activated = false;
			var level = SceneAs<Level>();
			// check if any other freezers/heaters are still running instead of inturrupting them,
			var similar = level.Tracker.GetEntities<TemperatureBlock>()
				.Cast<TemperatureBlock>()
				.Where(x => x != this && x.Target == Target && x.Activated)
				.ToList();
			if(similar.Count == 0)
				level.CoreMode = Previous;
			// remind them what the original core mode was though, if we need to switch
			if(Target != Previous)
				foreach(var block in similar)
					block.Previous = Previous;
		}
	}

	public override void Render(){
		base.Render();
		Draw.Rect(Collider, IsFreezer ? Color.SlateBlue : Color.SlateGray); // for now
		Draw.Rect(Position, Width, Height * (TimeLeft / MaxTime), IsFreezer ? Color.Blue : Color.Red);
	}

	protected DashCollisionResults OnDash(Player player, Vector2 direction){
		if(!Activated){
			var level = SceneAs<Level>();
			Previous = level.CoreMode;
			// if any temp blocks of the opposite type were activated *and* any wanted to change the core mode back, just turn them off
			// and don't activate
			var different = level.Tracker.GetEntities<TemperatureBlock>()
				.Cast<TemperatureBlock>()
				.Any(x => x.Target != Target && x.Activated && x.Previous != x.Target);
			
			if(Target != Previous){
				switching = true;
				level.CoreMode = Target;
				switching = false;
			}
			
			if(different)
				return DashCollisionResults.Rebound;
		}
		
		TimeLeft = MaxTime;
		Activated = true;
		return DashCollisionResults.Rebound;
	}

	protected void OnCoreModeSwitch(CoreMode next){
		if(!switching){
			Activated = false;
			TimeLeft = 0;
			// but don't switch back core mode
		}
	}
}