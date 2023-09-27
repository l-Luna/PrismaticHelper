using Celeste;
using Microsoft.Xna.Framework;

namespace PrismaticHelper.Entities.Objects;

public class ForceRoomTransitionTrigger : Trigger{
	
	public ForceRoomTransitionTrigger(EntityData data, Vector2 offset) : base(data, offset){}

	public override void OnEnter(Player player){
		base.OnEnter(player);
	}
}