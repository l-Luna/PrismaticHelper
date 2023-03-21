using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Panels;

public class WindowpaneManager : Entity{

	public static VirtualRenderTarget Displacement2;

	private readonly string RoomName;
	private readonly Level level;

	private bool active = true, bg = false;

	public static WindowpaneManager ofRoom(string roomName, Scene owner, bool bg){
		if(owner is not Level l)
			return null;

		var target = l.Session.MapData.Levels.Find(d => d.Name.Equals(roomName));
		if(target == null)
			return null;

		try{
			Windowpanes.ManipulateLevelLoads = true;

			Session fakeSession = new Session(l.Session.Area){
				Level = target.Name
			};

			LevelLoader fakeLevelLoader = new LevelLoader(fakeSession, target.DefaultSpawn);
			new DynamicData(fakeLevelLoader).Invoke("LoadingThread_Safe");
			Level fake = fakeLevelLoader.Level;
			fake.LoadLevel(Player.IntroTypes.None, true);
			fake.Wipe?.Cancel();
			fake.Update();

			foreach(var e in fake.Entities)
				if(e is CrystalStaticSpinner css)
					css.ForceInstantiate();

			foreach(var player in fake.Entities.FindAll<Player>()){
				player.Active = false;
				player.Visible = false;
			}

			Audio.SetCamera(l.Camera);
			new DynamicData(typeof(GameplayRenderer)).Set("instance", l.GameplayRenderer);

			return new WindowpaneManager(roomName, fake, bg);
		}finally{
			Windowpanes.ManipulateLevelLoads = false;
		}
	}

	public static void Unload(){
		Displacement2?.Dispose();
	}

	private WindowpaneManager(string roomName, Level scene, bool bg){
		RoomName = roomName;
		level = scene;
		this.bg = bg;

		Depth = bg ? 8500 : Depths.FGDecals - 1;

		SpeedrunToolInterop.IgnoreSaveState?.Invoke(this, true);
	}

	private void SetupLevel(){
		if(level != null)
			foreach(var e in level.Entities)
				e.SceneBegin(level);

		Displacement2 ??= VirtualContent.CreateRenderTarget("PrismaticHelper:displacement2", 320, 180);
	}

	private void TeardownLevel(){
		if(active && level != null){
			active = false;
			DynamicData levelData = DynamicData.For(level);
			levelData.Invoke("set_Focused", false);
			foreach(var e in level.Entities)
				e.SceneEnd(level);
			level.Background.Ended(level);
			level.Foreground.Ended(level);
		}
	}

	public override void Update(){
		base.Update();

		var levelEntities = level?.Entities;
		if(levelEntities != null)
			DynamicData.For(levelEntities).Invoke("Update");
	}

	public override void Added(Scene scene){
		base.Added(scene);
		SetupLevel();
	}

	public override void Removed(Scene scene){
		level?.UnloadLevel();
		TeardownLevel();
		base.Removed(scene);
	}

	public override void SceneEnd(Scene scene){
		level?.UnloadLevel();
		TeardownLevel();
		base.SceneEnd(scene);
	}

	public override void Render(){
		base.Render();
		if(Scene is not Level l)
			return;

		Camera camera = SceneAs<Level>().Camera;
		// i Love stencils
		var myPanes = SceneAs<Level>().Tracker.GetEntities<Windowpane>().Cast<Windowpane>()
			.Where(x => x.RoomName == RoomName);

		Draw.SpriteBatch.End();

		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.MaskRenderTarget);
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		Draw.SpriteBatch.Begin();
		foreach(var panel in myPanes)
			panel.DrawMask(camera);
		Draw.SpriteBatch.End();

		VirtualRenderTarget oldGm = GameplayBuffers.Gameplay, oldDisp = GameplayBuffers.Displacement;
		GameplayBuffers.Gameplay = Stencils.ObjectRenderTarget; GameplayBuffers.Displacement = Displacement2;

		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
		Engine.Graphics.GraphicsDevice.Clear(level.BackgroundColor);
		level.BeforeRender();

		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
		//level.Background.Render(level);
		StylegroundsPanelRenderer.RenderStylegroundsForRoom(RoomName, l.Background, l);
		Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.GameplayRenderer.Camera.Matrix);
		level.Entities.RenderExcept((int) Tags.HUD | (int) TagsExt.SubHUD);

		GameplayRenderer.End();
		level.Lighting.Render(level);
		level.Displacement.Render(level);
		//level.Foreground.Render(level);
		StylegroundsPanelRenderer.RenderStylegroundsForRoom(RoomName, l.Foreground, l);
		level.AfterRender();
		GameplayBuffers.Gameplay = oldGm; GameplayBuffers.Displacement = oldDisp;

		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
		Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Stencils.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
		Draw.SpriteBatch.Draw(Stencils.MaskRenderTarget, Vector2.Zero, Color.White);
		Draw.SpriteBatch.End();

		Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
		GameplayRenderer.Begin();
		Draw.SpriteBatch.Draw(Stencils.ObjectRenderTarget, camera.Position, Color.White);
	}
}