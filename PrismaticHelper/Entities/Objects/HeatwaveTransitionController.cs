using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Objects;

[CustomEntity("PrismaticHelper/HeatwaveTransitionController")]
public class HeatwaveTransitionController : Entity{

	public HeatwaveTransitionController(EntityData data, Vector2 offset) : base(data.Position + offset){}
	public HeatwaveTransitionController(){}
	
	public override void Update(){
		base.Update();
		Level l = SceneAs<Level>();
		foreach(var heatwave in l.Foreground.GetEach<HeatWave>())
			DynamicData.For(heatwave).Set("wasShow", false);
		foreach(var heatwave in l.Background.GetEach<HeatWave>())
			DynamicData.For(heatwave).Set("wasShow", false);
	}
}