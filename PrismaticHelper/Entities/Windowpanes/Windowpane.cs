using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Windowpanes;

[CustomEntity("PrismaticHelper/Windowpane")]
[Tracked]
public class Windowpane : Entity{

	public new int Width = 4, Height = 4;
	
	public readonly string Room;

	public Windowpane(EntityData data, Vector2 pos) : base(data.Position + pos){
		Room = data.Attr("room");
		Width = data.Width;
		Height = data.Height;
	}

	public override void Added(Scene scene){
		base.Added(scene);
		Windowpanes.RegisterManagerRequired(scene, Room);
	}

	public override void Awake(Scene scene){
		base.Awake(scene);
		Windowpanes.WpAwake(scene);
	}
}