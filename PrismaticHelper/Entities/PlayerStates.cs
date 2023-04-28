using Celeste;
using Microsoft.Xna.Framework;

namespace PrismaticHelper.Entities;

public class PlayerStates{

	public static int MarbleState = -1;
	
	public static void Load(){
		On.Celeste.Player.ctor += OnPlayerConstruct;
	}

	public static void Unload(){
		On.Celeste.Player.ctor -= OnPlayerConstruct;
	}

	private static void OnPlayerConstruct(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spritemode){
		orig(self, position, spritemode);
		MarbleState = self.StateMachine.AddState(null);
	}
}