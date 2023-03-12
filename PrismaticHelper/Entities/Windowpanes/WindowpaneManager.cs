using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Windowpanes;

public class WindowpaneManager : Entity{
	
	public static VirtualRenderTarget Displacement2;
	
	private readonly string RoomName; 
	private readonly Level level;

	private bool active = true;
	
	public static WindowpaneManager ofRoom(string roomName, Scene owner, bool bg){
		if(owner is not Level l)
			return null;
		
		var target = l.Session.MapData.Levels.Find(d => d.Name.Equals(roomName));
		if(target == null)
			return null;

		try{
			Windowpanes.IgnoreSessionStarts = true;
			
			Session fakeSession = new Session(l.Session.Area){
				Level = target.Name
			};

			string oldErr = LevelEnter.ErrorMessage;
			LevelEnter.ErrorMessage = "";
			LevelLoader fakeLevelLoader = new LevelLoader(fakeSession, target.DefaultSpawn);
			LevelEnter.ErrorMessage = oldErr;
			new DynamicData(fakeLevelLoader).Invoke("LoadingThread_Safe");
			Level fake = fakeLevelLoader.Level;
			fake.LoadLevel(Player.IntroTypes.None, true);
			fake.Update();
			
			Audio.SetCamera(l.Camera);
			new DynamicData(typeof(GameplayRenderer)).Set("instance", l.GameplayRenderer);
			
			return new WindowpaneManager(roomName, fake, bg);
		}finally{
			Windowpanes.IgnoreSessionStarts = false;
		}
	}

	public static void Unload(){
		Displacement2?.Dispose();
	}

	private WindowpaneManager(string roomName, Level scene, bool bg){
		RoomName = roomName;
		level = scene;
		
		Depth = bg ? 8500 : Depths.FGDecals + 1;
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
		level?.Update();
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
		//return;
		
		Camera camera = SceneAs<Level>().Camera;
		// i Love stencils
		var myPanes = SceneAs<Level>().Tracker.GetEntities<Windowpane>().Cast<Windowpane>()
			.Where(x => x.Room == RoomName);
		
		Draw.SpriteBatch.End();
		
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.MaskRenderTarget);
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		Draw.SpriteBatch.Begin();
		foreach(var panel in myPanes)
			Draw.Rect(panel.X - camera.Left, panel.Y - camera.Top,
				panel.Width, panel.Height, Color.White);
		Draw.SpriteBatch.End();
		
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		level.BeforeRender();
		
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
		VirtualRenderTarget oldGm = GameplayBuffers.Gameplay, oldDisp = GameplayBuffers.Displacement;
		GameplayBuffers.Gameplay = Stencils.ObjectRenderTarget; GameplayBuffers.Displacement = Displacement2;
		
		level.Background.Render(level);
		Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.GameplayRenderer.Camera.Matrix);
		level.Entities.RenderExcept((int) Tags.HUD | (int) TagsExt.SubHUD);
		
		GameplayBuffers.Gameplay = oldGm; GameplayBuffers.Displacement = oldDisp;
		GameplayRenderer.End();
		level.Lighting.Render(level);
		level.Foreground.Render(level);
		level.AfterRender();
		
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
		Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Stencils.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
		Draw.SpriteBatch.Draw(Stencils.MaskRenderTarget, Vector2.Zero, Color.White);
		Draw.SpriteBatch.End();
		
		Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
		GameplayRenderer.Begin();
		Draw.SpriteBatch.Draw(Stencils.ObjectRenderTarget, camera.Position, Color.White);
	}
}