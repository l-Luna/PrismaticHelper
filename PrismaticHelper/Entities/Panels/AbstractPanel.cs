using System;
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
	public Color Tint;
	public Image Image, Mask;
	public bool Attached;

	public Vector2? Origin = null;

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
		
		Depth = Foreground ? Depths.Enemy - 1 : Depths.BGTerrain + 1;
		
		string imageName = data.Attr("image");
		string maskName = data.Attr("mask");

		if(!string.IsNullOrEmpty(maskName)){
			Add(Mask = new Image(GFX.Game[maskName]));
			Mask.Visible = false;
		}
		if(!string.IsNullOrEmpty(imageName))
			Add(Image = new Image(GFX.Game[imageName]));

		Attached = data.Bool("attached");
		if(Attached){
			Collider = new Hitbox(Width, Height);
		
			Add(new StaticMover{
				SolidChecker = CollideCheck
			});
		}
	}

	public int RealWidth{
		get{
			if(Mask == null)
				return Width;
			return (int)Math.Max(Mask.Width, Width);
		}
	}
	
	public int RealHeight{
		get{
			if(Mask == null)
				return Height;
			return (int)Math.Max(Mask.Height, Height);
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
			var texture = Mask.Texture;
			texture.Draw(new Vector2(realX, realY) + Mask.Position, Origin ?? texture.Center, Tint * Opacity, Mask.Scale * Scale);
		}
	}

	private Rectangle Area(Camera c){
		return new Rectangle(
			(int)((X - c.Left - 320 / 2f) * ScrollX + 320 / 2f),
			(int)((Y - c.Top - 180 / 2f) * ScrollY + 180 / 2f),
			RealWidth,
			RealHeight
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
	public Vector2 ScBasePosition() => Vector2.Zero;
	public Vector2 ScBaseJustify() => Vector2.Zero;
}