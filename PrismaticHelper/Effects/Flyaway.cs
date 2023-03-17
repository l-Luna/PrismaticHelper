using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Effects;

[CustomBackdrop("PrismaticHelper/Flyaway")]
public class Flyaway : Backdrop{

	private static readonly Vector2 Centre = new(160, 90);
	
	private readonly List<Prop> Props = new();
	
	private readonly List<MTexture> propSprites;
	
	private class Prop{
		public MTexture texture;
		public Vector2 position;
		public float scale;
	}

	public Flyaway(BinaryPacker.Element e){
		propSprites = GFX.Game.GetAtlasSubtextures(e.Attr("textures"));
	}

	public override void Update(Scene scene){
		base.Update(scene);
		foreach(var prop in Props.ToArray()){
			if((prop.position - Centre).LengthSquared() < 20 * 20)
				Props.Remove(prop);
			prop.position += (Centre - prop.position) * Engine.DeltaTime * 6;
		}
		if(Props.Count < 150 && Calc.Random.Chance(0.8f))
			Props.Add(new Prop{
				texture = Calc.Random.Choose(propSprites),
				position = Centre.Rotate(Calc.Random.NextFloat((float)(Math.PI * 2))) + Centre,
				scale = 0.2f + Calc.Random.NextFloat(0.1f)
			});
	}

	public override void Render(Scene scene){
		base.Render(scene);
		foreach(var prop in Props)
			prop.texture.DrawCentered(prop.position, Color.White, (float)(prop.scale * Math.Sqrt((Centre - prop.position).Length() / Centre.Length())));
	}
}