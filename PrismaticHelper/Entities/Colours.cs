using Microsoft.Xna.Framework;

namespace PrismaticHelper.Entities;

public static class Colours{
	
	public static Color mul(Color l, Color r){
		return new Color((l.R / 255f) * (r.R / 255f), (l.G / 255f) * (r.G / 255f), (l.B / 255f) * (r.B / 255f), (l.A / 255f) * (r.A / 255f));
	}

	public static Color darken(Color c){
		return new Color(c.R * (0.6f/255f), c.G * (0.6f/255f), c.B * (0.6f/255f), 1);
	}
}