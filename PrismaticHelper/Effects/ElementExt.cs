using Microsoft.Xna.Framework;
using static Celeste.BinaryPacker;

namespace PrismaticHelper.Effects;

public static class ElementExt{

	public static Vector2 AttrVec(this Element element, string prefix, Vector2 defaultValue = default){
		return new(element.AttrFloat(prefix + "X", defaultValue.X), element.AttrFloat(prefix + "Y", defaultValue.Y));
	}
	
	public static Vector2 AttrPos(this Element element, Vector2 defaultValue = default){
		return new(element.AttrFloat("x", defaultValue.X), element.AttrFloat("y", defaultValue.Y));
	}
}