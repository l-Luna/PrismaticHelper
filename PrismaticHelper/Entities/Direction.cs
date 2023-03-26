using System;
using Celeste;
using Microsoft.Xna.Framework;

namespace PrismaticHelper.Entities;

public enum Direction{
	Up, Down, Left, Right
}

public static class DirectionMethods{
	public static Vector2 Offset(this Direction d){
		return d switch{
			Direction.Up => -Vector2.UnitY,
			Direction.Down => Vector2.UnitY,
			Direction.Left => -Vector2.UnitX,
			Direction.Right => Vector2.UnitX,
			_ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
		};
	}

	public static bool IsVertical(this Direction d){
		return d is Direction.Up or Direction.Down;
	}
	
	public static bool IsHorizontal(this Direction d){
		return d is Direction.Left or Direction.Right;
	}

	public static Direction? AttrDirection(this EntityData data, string name, Direction? def = null){
		return data.Attr(name, null)?.ToLowerInvariant() switch{
			"up" => Direction.Up,
			"down" => Direction.Down,
			"left" => Direction.Left,
			"right" => Direction.Right,
			_ => def
		};
	}
}