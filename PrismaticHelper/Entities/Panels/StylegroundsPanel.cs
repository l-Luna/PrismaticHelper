using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Panels;

[CustomEntity("PrismaticHelper/StylegroundsPanel")]
[Tracked]
public class StylegroundsPanel : AbstractPanel{
	
	public StylegroundsPanel(EntityData data, Vector2 pos) : base(data, pos){}
}