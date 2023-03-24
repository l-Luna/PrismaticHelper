using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Cutscenes;

[Tracked]
public class Puppet : Entity, Scriptable{

	private Sprite sprite;
	private string name;
	private float scale;

	public Puppet(string name, string sprite){
		if(name == null || name.Trim().Length == 0)
			throw new ArgumentException("Puppet name must be non-null and non-empty");
		this.name = name;

		if(sprite != null && sprite.Trim().Length > 0)
			Add(this.sprite = GFX.SpriteBank.Create(sprite.Trim()));
	}

	// Scriptable
	public string ScName() => name;
	public Sprite? ScSprite() => sprite;
	public Vector2 ScBasePosition() => ScSprite()?.Center ?? Vector2.Zero;
	public Vector2 ScBaseJustify() => ScBasePosition();
	
	public Vector2 ScPosition{
		get => Position;
		set => Position = value;
	}
	public float ScScale{
		get => scale;
		set => scale = value;
	}
}