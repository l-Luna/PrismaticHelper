using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Objects;

[CustomEntity("PrismaticHelper/CustomHangingLamp")]
public class CustomHangingLamp : HangingLamp{

	private readonly DynamicData selfData;
	
	public CustomHangingLamp(EntityData e, Vector2 position) : base(e, e.Position + position){
		selfData = new DynamicData(this);
		
		string sprite = e.Attr("sprite");
		if(!string.IsNullOrWhiteSpace(sprite)){
			var images = selfData.Get<List<Image>>("images");
			foreach(var image in images)
				image.RemoveSelf();
			images.Clear();

			// ... vanilla copy from HangingLamp ctor
			MTexture texture = GFX.Game[sprite];
			for(int idx = 0; idx < Length - 8; idx += 8){
				Image chainImg;
				Add(chainImg = new Image(texture.GetSubtexture(0, 8, 8, 8)));
				chainImg.Origin.X = 4f;
				chainImg.Origin.Y = -idx;
				images.Add(chainImg);
			}

			Image baseImg;
			Add(baseImg = new Image(texture.GetSubtexture(0, 0, 8, 8)));
			baseImg.Origin.X = 4f;
			Image headImg;
			Add(headImg = new Image(texture.GetSubtexture(0, 16, 8, 8)));
			headImg.Origin.X = 4f;
			headImg.Origin.Y = -(Length - 8);
			images.Add(headImg);
		}

		bool attached = e.Bool("attached");
		if(attached)
			Add(new StaticMover{
				SolidChecker = solid => CollideCheckOutside(solid, Position - Vector2.UnitY)
			});

		var vertexLight = selfData.Get<VertexLight>("light");
		Color glowColour = e.HexColor("glowColour", Color.White);
		if(glowColour != Color.White){
			vertexLight.Color = glowColour;
			vertexLight.Dirty = true;
		}

		vertexLight.StartRadius = e.Float("glowStartRadius", 24);
		vertexLight.EndRadius = e.Float("glowEndRadius", 48);
		selfData.Get<BloomPoint>("bloom").Radius = e.Float("glowEndRadius", 48);
	}
}