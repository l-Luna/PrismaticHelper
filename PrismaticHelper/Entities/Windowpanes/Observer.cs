using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Windowpanes;

[CustomEntity("PrismaticHelper/Observer")]
[TrackedAs(typeof(Player))]
public class Observer : Player{
	
	public Observer(EntityData data, Vector2 pos) : base(data.Position + pos, PlayerSpriteMode.Playback){}
}