using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Cutscenes;

public interface Scriptable{

	public string ScName();

	public Sprite? ScSprite();

	public Vector2 ScPosition{ get; set; }
	public float ScScale{ get; set; }

	public Vector2 ScBasePosition();
	public Vector2 ScBaseJustify();
}