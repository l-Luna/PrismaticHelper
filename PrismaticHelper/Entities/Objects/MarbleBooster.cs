using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Objects;

[CustomEntity("PrismaticHelper/MarbleBooster")]
public class MarbleBooster : Entity{

	private const string CURRENT_MARBLE_ID = "PrismaticHelper:current_marble_booster";

	protected Sprite sprite;
	protected int damage = 0;
	
	public MarbleBooster(EntityData data, Vector2 offset) : base(data.Position + offset){
		Depth = Depths.Above;
		Collider = new Circle(10f, y: 2f);
		
		Add(new PlayerCollider(OnPlayer));
		Add(sprite = GFX.SpriteBank.Create("booster"));
	}

	protected void OnPlayer(Player p){
		p.StateMachine.State = PlayerStates.MarbleState;
		SetCurrentMarble(p, this);
	}

	public static int MarbleUpdate(Player self){
		// ...
		
		return PlayerStates.MarbleState;
	}

	public static void MarbleBegin(Player self){
		
	}

	public static void MarbleEnd(Player self){
		
	}

	public static MarbleBooster? CurrentMarble(Player player)
		=> DynamicData.For(player).TryGet<MarbleBooster>(CURRENT_MARBLE_ID, out var marble) ? marble : null;

	public static void SetCurrentMarble(Player player, MarbleBooster marble)
		=> DynamicData.For(player).Set(CURRENT_MARBLE_ID, marble);
}