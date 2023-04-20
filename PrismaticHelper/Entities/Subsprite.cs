using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities;

/// Like a Sprite, but for a subtexture
public class Subsprite : Component{

	protected Sprite Inner;
	protected Rectangle Subsection;

	public Vector2 Position;
	
	public Subsprite(Sprite inner, Rectangle subsection) : base(true, true){
		Inner = inner;
		Subsection = subsection;
	}

	public override void Render(){
		base.Render();
		Inner.DrawSubrect(Position, Subsection);
	}
}