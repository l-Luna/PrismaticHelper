using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Panels;

[CustomEntity("PrismaticHelper/StylegroundsPanel")]
[Tracked]
public class StylegroundsPanel : Entity{
	
	public new int Width = 4, Height = 4;

	public bool Foreground = false;

	public float ScrollX = 1f, ScrollY = 1f, Opacity = 1f;

	public string Room;

	public Color Tint;

	public StylegroundsPanel(EntityData data, Vector2 pos) : base(data.Position + pos){
		Width = data.Width;
		Height = data.Height;
		Foreground = data.Bool("foreground", false);
		ScrollX = data.Float("scrollX", 1);
		ScrollY = data.Float("scrollY", 1);
		Opacity = data.Float("opacity", 1);
		Room = data.Attr("room");
		Tint = Calc.HexToColor(data.Attr("tint", "#ffffff"));
	}

	public Rectangle Area(Camera c){
		return new Rectangle(
			(int)((X - c.Left - 320 / 2f) * ScrollX + 320 / 2f),
			(int)((Y - c.Top - 180 / 2f) * ScrollY + 180 / 2f),
			Width,
			Height
		);
	}

	public bool OnScreen(Camera c){
		var area = Area(c);
		return (area.Left < c.Right || area.Right > c.Left) && (area.Top < c.Bottom || area.Bottom > c.Top);
	}
}