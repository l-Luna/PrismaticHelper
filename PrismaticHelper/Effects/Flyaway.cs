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
		public bool flipX, flipY;
	}

	public Flyaway(BinaryPacker.Element e){
		propSprites = GFX.Game.GetAtlasSubtextures(e.Attr("textures"));
	}

	public override void Update(Scene scene){
		base.Update(scene);
		foreach(var prop in Props.ToArray()){
			if((prop.position - Centre).LengthSquared() < 5 * 5)
				Props.Remove(prop);
			prop.position += (Centre - prop.position) * Engine.DeltaTime;
		}

		if(Props.Count < 170)
			if(Calc.Random.Chance(0.5f))
			//for(int i = 0; i < Calc.Random.Next(1, 2); i++)
				Props.Add(new Prop{
					texture = Calc.Random.Choose(propSprites),
					position = (Centre.Rotate(Calc.Random.NextFloat((float)(Math.PI * 2))) * 1.5f) + Centre,
					scale = 0.2f + Calc.Random.NextFloat(0.1f),
					flipX = Calc.Random.Chance(0.5f),
					flipY = Calc.Random.Chance(0.5f)
				});
	}

	public override void Render(Scene scene){
		base.Render(scene);
		foreach(var prop in Props){
			var scale = prop.scale * ((Centre - prop.position).Length() / Centre.Length());
			prop.texture.DrawCentered(
				prop.position,
				Color.White * FadeAlphaMultiplier * scale * 1.4f,
				new Vector2(scale * (prop.flipX ? -1 : 1), scale * (prop.flipY ? -1 : 1))
			);
		}
	}
}