using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Panels;

[CustomEntity("PrismaticHelper/Windowpane", "PrismaticHelper/WorldPanel")]
[Tracked]
public class WorldPanel : AbstractPanel{

	public bool IgnoreColours;

	public WorldPanel(EntityData data, Vector2 pos) : base(data, pos){
		SpeedrunToolInterop.IgnoreSaveState?.Invoke(this, true);
	}

	public override void Added(Scene scene){
		base.Added(scene);
		WorldPanels.RegisterManagerRequired(scene, RoomName, !Foreground, IgnoreColours);
	}

	public override void Awake(Scene scene){
		base.Awake(scene);
		WorldPanels.WpAwake(scene);
	}
}