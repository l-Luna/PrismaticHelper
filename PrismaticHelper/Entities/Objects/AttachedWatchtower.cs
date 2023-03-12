using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Objects;

[TrackedAs(typeof(Lookout))]
[CustomEntity("PrismaticHelper/AttachedWatchtower")]
public class AttachedWatchtower : Lookout{

	public bool VisibleWhenDisabled = false;
	public Color ColourWhenDisabled = Color.Gray;
	
	private bool enabled = true;
	private StaticMover mover;
	
	public AttachedWatchtower(EntityData data, Vector2 offset) : base(data, offset){
		Add(mover = new StaticMover{
			SolidChecker = solid => CollideCheckOutside(solid, Position + Vector2.UnitY),
			JumpThruChecker = jumpThru => CollideCheckOutside(jumpThru, Position + Vector2.UnitY),
			OnEnable = OnEnable,
			OnDisable = OnDisable
		});
	}

	public override void Awake(Scene scene){
		base.Awake(scene);
		if(mover.Platform is CassetteBlock cb){
			VisibleWhenDisabled = true;
			ColourWhenDisabled = Colours.mul(Calc.HexToColor("667da5"), DynamicData.For(cb).Get<Color>("color"));
		}
	}

	public override void Update(){
		base.Update();
		var talkComponentUi = DynamicData.For(this).Get<TalkComponent>("talk").UI;
		if(talkComponentUi != null)
			talkComponentUi.Visible = !CollideCheck<Solid>() && enabled;
	}

	private void OnEnable(){
		Visible = enabled = true;
		DynamicData.For(this).Get<Sprite>("sprite").Color = Color.White;
	}

	private void OnDisable(){
		enabled = false;
		if(VisibleWhenDisabled)
			DynamicData.For(this).Get<Sprite>("sprite").Color = ColourWhenDisabled;
		else
			Visible = false;
	}
}