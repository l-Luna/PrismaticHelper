using System;
using System.Linq;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace PrismaticHelper.Triggers;

[CustomEntity("PrismaticHelper/UnderwaterInteractionTrigger")]
[Tracked]
public class UnderwaterInteractionTrigger : Trigger{
	
	public UnderwaterInteractionTrigger(EntityData data, Vector2 offset) : base(data, offset){}

	public static void Load(){
		IL.Celeste.TalkComponent.Update += ModTalkComponentUpdate;
	}

	public static void Unload(){
		IL.Celeste.TalkComponent.Update -= ModTalkComponentUpdate;
	}
	
	private static void ModTalkComponentUpdate(ILContext il){
		ILCursor cursor = new(il);
		// OnGround() => OnGround() | swimming; outside of water, it checks anyways for non-swim state
		// callvirt instance bool Celeste.Actor::OnGround(int32)
		if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Actor>("OnGround"))){
			cursor.Emit(OpCodes.Ldloc_0); // player
			cursor.EmitDelegate<Func<bool, Player, bool>>((old, player) => old || player.StateMachine.State == Player.StSwim);
		}else{
			PrismaticHelperModule.LogError("Failed to IL hook for UnderwaterInteractionTrigger (1/3)!");
			return;
		}

		// swap state 0 (normal) for swim
		// ldfld class Monocle.StateMachine Celeste.Player::StateMachine
		// callvirt instance int32 Monocle.StateMachine::get_State()
		if(cursor.TryGotoNext(MoveType.After,
			   instr => instr.MatchLdfld<Player>("StateMachine"),
			   instr => instr.MatchCallvirt<StateMachine>("get_State"))){
			// short-circuits on true
			cursor.Emit(OpCodes.Ldloc_0); // player
			cursor.EmitDelegate<Func<bool, Player, bool>>((old, player) => {
				var canActivateWet = player.Scene.Tracker.GetEntities<UnderwaterInteractionTrigger>()
					.Cast<UnderwaterInteractionTrigger>()
					.Any(x => x.PlayerIsInside);
				return canActivateWet ? player.StateMachine.State != Player.StSwim : old;
			});
		}else{
			PrismaticHelperModule.LogError("Failed to IL hook for UnderwaterInteractionTrigger (2/3)!");
			return;
		}
		// and again
		// ldfld class Monocle.StateMachine Celeste.Player::StateMachine
		// call int32 Monocle.StateMachine::op_Implicit(class Monocle.StateMachine)
		int count = 0;
		while(cursor.TryGotoNext(MoveType.After,
			   instr => instr.MatchLdfld<Player>("StateMachine"),
			   instr => instr.MatchCall<StateMachine>("op_Implicit"))){
			// short-circuits on true
			cursor.Emit(OpCodes.Ldloc_0); // player
			cursor.EmitDelegate<Func<bool, Player, bool>>((old, player) => {
				var canActivateWet = player.Scene.Tracker.GetEntities<UnderwaterInteractionTrigger>()
					.Cast<UnderwaterInteractionTrigger>()
					.Any(x => x.PlayerIsInside);
				return canActivateWet ? player.StateMachine.State != Player.StSwim : old;
			});
			count++;
		}
		if(count != 2)
			PrismaticHelperModule.LogError("Failed to IL hook for UnderwaterInteractionTrigger (3/3)!");
	}
}