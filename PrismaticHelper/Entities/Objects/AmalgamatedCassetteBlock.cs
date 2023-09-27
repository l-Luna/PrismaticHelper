using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using SimplexNoise;

namespace PrismaticHelper.Entities.Objects;

[CustomEntity("PrismaticHelper/AmalgamatedCassetteBlock")]
[Tracked]
public class AmalgamatedCassetteBlock : Solid{
	protected readonly List<int> Indices = new();
	protected readonly List<Color> Colors = new();

	protected int curIndex;
	protected Wiggler scaleWiggler;
	protected bool activated;
	protected BoxSide side;
	protected int blockHeight = 0;
	protected Sprite sprite;

	public AmalgamatedCassetteBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false){
		Collidable = false;

		foreach(string idxStr in data.Attr("indices").Split(','))
			if(int.TryParse(idxStr, out int i))
				Indices.Add(i);
		foreach(string colStr in data.Attr("colors").Split(','))
			Colors.Add(Calc.HexToColor(colStr));

		Add(scaleWiggler = Wiggler.Create(0.3f, 3f));

		Add(new CassetteListener{
			PreBeat = PreBeat,
			OnBeat = OnBeat,
			OnSilentBeat = i => {
				OnBeat(i);
				if(!activated)
					ShiftSize(2);
			},
			OnFinish = () => activated = false
		});
	}

	public override void Awake(Scene scene){
		base.Awake(scene);
		scene.Add(side = new BoxSide(this));

		Add(sprite = GFX.SpriteBank.Create("PrismaticHelper_amalgamated_cassette_block"));
		NinePatch.CreateConnectedNinepatch(this, sprite, NinePatch.TileSpec.CassetteLike, block =>
			block.Indices.Intersect(Indices).Count() == Indices.Count);
	}

	public override void Render(){
		Draw.SpriteBatch.End();
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.MaskRenderTarget);
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		GameplayRenderer.Begin();
		
		// the mask...
		int stretch = (int)(scaleWiggler.Value * 2);
		Rectangle area = new Rectangle(Collider.Bounds.X, Collider.Bounds.Y - stretch, Collider.Bounds.Width, Collider.Bounds.Height + stretch);
		bool first = true;
		for(var i = 0; i < Indices.Count; i++){
			Color c = Colors.Count > i ? Colors[i] : Color.White;
			if(!Collidable || curIndex != i) c = Colours.darken(c);
			if(first){
				Draw.Rect(area, c);
				first = false;
			}else{
				for(int x = area.X; x < area.Right; x++)
					for(int y = area.Y; y < area.Bottom; y++){
						float at = Noise.Generate(x / 20f + i * 500, y / 20f, Scene.TimeActive / 2f);
						if(at > 0)
							Draw.Point(new Vector2(x, y), c);
					}
			}
		}

		if(first)
			Draw.Rect(area, Color.White);
		
		GameplayRenderer.End();
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		GameplayRenderer.Begin();
		// the object...
		base.Render();
		// boilerplate...
		Draw.SpriteBatch.End();
		// ap
		Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Stencils.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
		Draw.SpriteBatch.Draw(Stencils.MaskRenderTarget, Vector2.Zero, Color.White);
		Draw.SpriteBatch.End();
		// render the styleground
		Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
		GameplayRenderer.Begin();
		Draw.SpriteBatch.Draw(Stencils.ObjectRenderTarget, SceneAs<Level>().Camera.Position, Color.White);
	}

	public override void Update(){
		base.Update();
		if(activated && !Collidable){
			Collidable = true;
			ShiftSize(-1);
			scaleWiggler.Start();
		}else if(!activated && Collidable){
			Collidable = false;
			ShiftSize(1);
		}
	}

	private void PreBeat(int idx){
		if(Collidable ^ Indices.Contains(idx))
			ShiftSize(Collidable ? 1 : -1);
	}

	private void OnBeat(int i){
		if(Indices.Contains(i)){
			activated = true;
			curIndex = Indices.IndexOf(i);
		} else
			activated = false;
	}

	private void ShiftSize(int amount){
		MoveV(amount);
		blockHeight -= amount;
	}

	private Color CurColor() => curIndex < Colors.Count ? Colors[curIndex] : Colors.LastOrDefault();

	protected sealed class BoxSide : Entity{
		private readonly AmalgamatedCassetteBlock block;

		public BoxSide(AmalgamatedCassetteBlock block){
			this.block = block;
		}

		public override void Render() => Draw.Rect(block.X, block.Y + block.Height - 8, block.Width, 8 + block.blockHeight, Colours.darken(block.CurColor()));
	}
}