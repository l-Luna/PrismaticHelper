using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Panels;

[CustomEntity("PrismaticHelper/Windowpane")]
[Tracked]
public class Windowpane : AbstractPanel{

	public Windowpane(EntityData data, Vector2 pos) : base(data, pos){
		SpeedrunToolInterop.IgnoreSaveState?.Invoke(this, true);
	}

	public override void Added(Scene scene){
		base.Added(scene);
		Windowpanes.RegisterManagerRequired(scene, RoomName, !Foreground);
	}

	public override void Awake(Scene scene){
		base.Awake(scene);
		Windowpanes.WpAwake(scene);
	}
}