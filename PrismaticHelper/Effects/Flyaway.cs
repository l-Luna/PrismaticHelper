using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Effects;

[CustomBackdrop("PrismaticHelper/Flyaway")]
public class Flyaway : Backdrop{
	
	public readonly int Limit;
	public readonly float Chance, SpawnDistance, BaseScale, BonusScale, BaseAlpha, RotationSpeed;
	public new readonly float Speed;
	public readonly bool CanFlipX, CanFlipY, ScaleAlpha;
	public readonly Vector2 Centre;
	
	protected readonly List<Prop> props = new();
	protected readonly List<MTexture> propSprites;
	
	protected class Prop{
		public MTexture texture;
		public Vector2 position;
		public float scale;
		public bool flipX, flipY;
	}

	public Flyaway(BinaryPacker.Element e){
		Limit = e.AttrInt("limit", 170);
		
		Chance = e.AttrFloat("chance", 0.5f);
		Speed = e.AttrFloat("speed", 1);
		RotationSpeed = e.AttrFloat("rotationSpeed", 0);
		SpawnDistance = e.AttrFloat("spawnDistance", 1.5f);
		BaseScale = e.AttrFloat("baseScale", 1f);
		BonusScale = e.AttrFloat("bonusScale", 0.5f);
		BaseAlpha = e.AttrFloat("baseAlpha", 1);
		
		CanFlipX = e.AttrBool("canFlipX", true);
		CanFlipY = e.AttrBool("canFlipY", true);
		ScaleAlpha = e.AttrBool("scaleAlpha", true);

		Centre = e.AttrVec("centre", new(160, 90));

		if(SpawnDistance <= 0.05f)
			SpawnDistance = 0.05f;

		propSprites = GFX.Game.GetAtlasSubtextures(e.Attr("textures"));
	}

	public override void Update(Scene scene){
		base.Update(scene);
		foreach(var prop in props.ToArray()){
			var dist = (prop.position - Centre).LengthSquared();
			if((Speed > 0 && dist < 5 * 5) || (Speed < 0 && dist > Centre.LengthSquared() * 1.3))
				props.Remove(prop);
			prop.position += (Centre - prop.position) * Engine.DeltaTime * Speed;
			if(RotationSpeed != 0)
				prop.position = (prop.position - Centre).Rotate(Engine.DeltaTime * RotationSpeed) + Centre;
		}

		if(props.Count < Limit)
			if(Calc.Random.Chance(Chance))
				props.Add(new Prop{
					texture = Calc.Random.Choose(propSprites),
					position = (Centre.Rotate(Calc.Random.NextFloat((float)(Math.PI * 2))) * SpawnDistance) + Centre,
					scale = BaseScale + Calc.Random.NextFloat(BonusScale),
					flipX = CanFlipX && Calc.Random.Chance(0.5f),
					flipY = CanFlipY && Calc.Random.Chance(0.5f)
				});
	}

	public override void Render(Scene scene){
		base.Render(scene);
		foreach(var prop in props){
			var scale = prop.scale * ((Centre - prop.position).Length() / Centre.Length());
			prop.texture.DrawCentered(
				prop.position,
				Color.White * FadeAlphaMultiplier * (ScaleAlpha ? scale : 1) * BaseAlpha,
				new Vector2(scale * (prop.flipX ? -1 : 1), scale * (prop.flipY ? -1 : 1))
			);
		}
	}
}