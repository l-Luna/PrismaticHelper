﻿using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Objects;

[CustomEntity("PrismaticHelper/CustomPowerSourceNumber")]
public class CustomPowerSourceNumber : Entity{
	
	private readonly Image image;
	private readonly Image glow;

	private float ease;
	private float timer;

	private EntityID cond;
	private bool requiresLightningDisabled;

	public CustomPowerSourceNumber(EntityData data, Vector2 offset){
		Position = data.Position + offset;
		Depth = -10010;
		Add(image = new Image(GFX.Game[data.Attr("base", "scenery/powersource_numbers/1")]));
		Add(glow = new Image(GFX.Game[data.Attr("glow", "scenery/powersource_numbers/1_glow")]));
		requiresLightningDisabled = data.Bool("requiresLightningDisabled", true);
		glow.Color = Color.Transparent;

		string[] array = data.Attr("cond").Split(':');
		cond = new EntityID{
			Level = array[0],
			ID = Convert.ToInt32(array[1])
		};
	}

	public override void Update(){
		base.Update();
		if(!requiresLightningDisabled || (Scene as Level).Session.GetFlag("disable_lightning")){
			if(!SceneAs<Level>().Session.DoNotLoad.Contains(cond)){
				timer += Engine.DeltaTime;
				ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 4);
			}else
				ease = Calc.Approach(ease, 0f, Engine.DeltaTime * 2);

			glow.Color = Color.White * ease * Calc.SineMap(timer * 2f, 0.5f, 0.9f);
		}
	}
}