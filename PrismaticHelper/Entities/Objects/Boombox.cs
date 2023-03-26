using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities.Objects;

[CustomEntity("PrismaticHelper/Boombox")]
public class Boombox : Entity{
	
	protected int Index;
	protected Direction? Direction;
	protected bool Big, Attached;
	
	protected StaticMover mover;
	protected Hitbox attachCollider;
	protected Circle explodeCollider;
	protected Image image;

	protected ParticleType pBoom;
	
	public Boombox(EntityData data, Vector2 pos) : base(data.Position + pos){
		Index = data.Int("index");
		Direction = data.AttrDirection("direction");
		Big = data.Bool("big");
		Attached = data.Bool("attached");

		int size = Big ? 32 : 24;
		Collider = attachCollider = new Hitbox(size, size);
		explodeCollider = new Circle(size * 1.5f, size / 2f, size / 2f + 4);
		
		Add(new CassetteListener{
			PreBeat = PreBeat,
			OnBeat = OnBeat
		});
		
		Add(image = new Image(GFX.Game["PrismaticHelper/boombox/solid" + (Big ? "_big0" : "0") + Math.Min(Index, 3)]));
		var color = CassetteListener.GetByIndex(Index);
		image.Color = color;
		pBoom = new ParticleType{
			Color = color,
			Color2 = Colours.mul(Calc.HexToColor("667da5"), color),
			ColorMode = ParticleType.ColorModes.Blink,
			FadeMode = ParticleType.FadeModes.Late,
			Size = 1f,
			LifeMin = 0.4f,
			LifeMax = 1.0f,
			SpeedMin = 10f,
			SpeedMax = 30f,
			SpeedMultiplier = 0.3f,
			DirectionRange = 1.0471976f
		};

		if(Attached){
			Add(mover = new StaticMover{
				SolidChecker = solid => CollideCheck(solid, Position + (Direction ?? Entities.Direction.Down).Offset()),
				JumpThruChecker = jumpThru => CollideCheck(jumpThru, Position + Direction?.Offset() ?? Vector2.Zero),
				OnShake = v => image.Position += v
				//OnEnable = OnEnable,
				//OnDisable = OnDisable
			});
		}
	}

	protected void PreBeat(int idx){
		if(idx == Index)
			image.Position += Vector2.UnitY;
	}

	protected void OnBeat(int idx){
		if(idx == Index){
			image.Position -= Vector2.UnitY;
			
			Level level = SceneAs<Level>();
			var radius = explodeCollider.Radius;
			
			Collider = explodeCollider;
			Player player = CollideFirst<Player>();
			if(player != null && !Scene.CollideCheck<Solid>(Position, player.Center)){
				var snapUp = Direction?.IsVertical() ?? true;
				var sidesOnly = Direction?.IsHorizontal() ?? false;
				if(Big) // TODO: proper directional big boxes
					player.ExplodeLaunch(Center, snapUp, sidesOnly);
				else{
					// just a small explosion without refills
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
					Celeste.Celeste.Freeze(0.1f);
					Vector2 vector2 = (player.Center - Center).SafeNormalize(-Vector2.UnitY);
					float dot = Vector2.Dot(vector2, Vector2.UnitY);
					if(snapUp && dot <= -0.7){
						vector2.X = 0.0f;
						vector2.Y = -1f;
					}else if(dot <= 0.65 && dot >= -0.55){
						vector2.Y = 0.0f;
						vector2.X = Math.Sign(vector2.X);
					}
					if(sidesOnly && vector2.X != 0.0){
						vector2.Y = 0.0f;
						vector2.X = Math.Sign(vector2.X);
					}
					
					player.Speed = 250 * vector2;
					if(player.Speed.Y <= 50.0){
						player.Speed.Y = Math.Min(-150f, player.Speed.Y);
						player.AutoJump = true;
					}
					
					SlashFx.Burst(player.Center, player.Speed.Angle());
					player.StateMachine.State = 7;
				}
			}

			level.Displacement.AddBurst(Center, 0.4f, 12f, radius, 0.2f);

			for(float direction = 0; direction < 2 * Math.PI; direction += 0.19f){
				Vector2 position = Center + Calc.AngleToVector(direction + Calc.Random.Range(-1 * (float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(3, 7));
				level.Particles.Emit(pBoom, position, direction);
			}
			
			Collider = attachCollider;
		}
	}

	public override void DebugRender(Camera camera){
		base.DebugRender(camera);
		var tmp = Collider;
		Collider = explodeCollider;
		explodeCollider.Render(camera, Color.Pink);
		Collider = tmp;
	}
}