using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Windowpanes;

[Tracked]
public class WindowpaneManager : Entity{

	private readonly string RoomName;
	private readonly Level level;

	private bool active = true;
	
	public static WindowpaneManager ofRoom(string roomName, Scene owner){
		if(owner is not Level l)
			return null;
		
		var target = l.Session.MapData.Levels.Find(d => d.Name.Equals(roomName));
		if(target == null)
			return null;

		try{
			Windowpanes.IgnoreSessionStarts = true;

			var oldTiler = GFX.FGAutotiler;
			
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
			
			Audio.SetCamera(((Level)owner).Camera);
			GFX.FGAutotiler = oldTiler;
			new DynamicData(typeof(GameplayRenderer)).Set("instance", l.GameplayRenderer);
			
			return new WindowpaneManager(roomName, fake);
		}finally{
			Windowpanes.IgnoreSessionStarts = false;
		}
	}

	private WindowpaneManager(string roomName, Level scene){
		RoomName = roomName;
		level = scene;
	}

	private void SetupLevel(){
		if(level != null)
			foreach(var e in level.Entities)
				e.SceneBegin(level);
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
			Windowpanes.WpRemoved(Scene);
		}
	}

	public override void Update(){
		base.Update();
		//level.Update();
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
		var myPanes = SceneAs<Level>().Tracker.GetEntities<Windowpane>()
			.Cast<Windowpane>()
			.Where(x => x.Room == RoomName);
		
		Draw.SpriteBatch.End();
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.MaskRenderTarget);
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		Draw.SpriteBatch.Begin();
		foreach(var panel in myPanes)
			Draw.Rect(panel.X - camera.Left, panel.Y - camera.Top,
				panel.Width, panel.Height, Color.White);
		Draw.SpriteBatch.End();
		
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		Windowpanes.CurrentSubstitute = Stencils.ObjectRenderTarget;
		level.BeforeRender();
		level.Render();
		level.AfterRender();
		Windowpanes.CurrentSubstitute = null;
		
		Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Stencils.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
		Draw.SpriteBatch.Draw(Stencils.MaskRenderTarget, Vector2.Zero, Color.White);
		Draw.SpriteBatch.End();
		
		Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
		GameplayRenderer.Begin();
		Draw.SpriteBatch.Draw(Stencils.ObjectRenderTarget, camera.Position, Color.White);
		// GameplayRenderer.End();
		
	}
}