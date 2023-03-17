using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using PrismaticHelper.Entities.Cutscenes;

namespace PrismaticHelper.Entities.Panels;

public abstract class AbstractPanel : Entity, Scriptable{

	public new int Width, Height;
	
	public string RoomName, Name;
	public bool Foreground;
	public float ScrollX, ScrollY, Opacity, Scale = 1;
	public string Mask;
	public Color Tint;
	public Image Image;
	public bool Attached;

	protected AbstractPanel(EntityData data, Vector2 pos) : base(data.Position + pos){
		Width = data.Width;
		Height = data.Height;
		Foreground = data.Bool("foreground");
		ScrollX = data.Float("scrollX", 1);
		ScrollY = data.Float("scrollY", 1);
		Opacity = data.Float("opacity", 1);
		RoomName = data.Attr("room");
		Name = data.Attr("name");
		Tint = Calc.HexToColor(data.Attr("tint", "#ffffff"));
		
		Depth = Foreground ? Depths.FGDecals + 1 : 8500;
		
		var image = data.Attr("image");
		Mask = data.Attr("mask");

		if(Mask.Trim() == "")
			Mask = null;
		if(image.Trim() == "")
			image = null;

		if(image != null)
			Add(Image = new Image(GFX.Game[image]));

		Attached = data.Bool("attached");
		if(Attached){
			Collider = new Hitbox(Width, Height);
		
			Add(new StaticMover{
				SolidChecker = CollideCheck
			});
		}
	}

	public void DrawMask(Camera camera){
		if(Tint.A == 0 || Opacity == 0)
			return;
		
		var realX = (X - camera.Left - 160) * ScrollX + 160;
		var realY = (Y - camera.Top - 90) * ScrollY + 90;
		if(Mask == null)
			Draw.Rect(realX, realY, Width, Height, Tint * Opacity);
		else{
			var texture = GFX.Game[Mask];
			texture.Draw(new Vector2(realX, realY), new Vector2(texture.Width, texture.Height) / 2, Tint * Opacity, Scale);
		}
	}

	private Rectangle Area(Camera c){
		return new Rectangle(
			(int)((X - c.Left - 320 / 2f) * ScrollX + 320 / 2f),
			(int)((Y - c.Top - 180 / 2f) * ScrollY + 180 / 2f),
			Width,
			Height
		);
	}
	
	public bool VisibleOnScreen(Camera c){
		if(Tint.A == 0 || Opacity == 0)
			return false;

		var area = Area(c);
		return (area.Left < c.Right || area.Right > c.Left) && (area.Top < c.Bottom || area.Bottom > c.Top);
	}

	// Scriptable
	public string ScName() => Name;
	public Sprite? ScSprite() => null;
	public Vector2 ScPosition{ get => Position; set => Position = value; }
	public float ScScale{ get => Scale; set => Scale = value; }
}