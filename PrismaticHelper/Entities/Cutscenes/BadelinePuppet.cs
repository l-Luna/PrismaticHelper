using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Cutscenes;

[Tracked]
public class BadelinePuppet : BadelineDummy, Scriptable{

	private string name;

	public BadelinePuppet(Vector2 position, string name) : base(position){
		this.name = name;
	}

	public string ScName() => name;
	public Sprite ScSprite() => Sprite;

	public Vector2 ScPosition{
		get => Position;
		set => Position = value;
	}
	
	public float ScScale{ get; set; } // TODO?
	public Vector2 ScBasePosition() => Vector2.Zero;
	public Vector2 ScBaseJustify() => new Vector2(15, 25);
}