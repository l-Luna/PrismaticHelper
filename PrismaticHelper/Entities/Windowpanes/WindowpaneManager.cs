using System;
using System.Linq;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Windowpanes;

[Tracked]
public class WindowpaneManager : SceneWrappingEntity<Level>{

	private readonly string RoomName;
	
	public static WindowpaneManager ofRoom(string roomName, Scene owner){
		if(owner is not Level l)
			return null;
		
		var target = l.Session.MapData.Levels.Find(d => d.Name.Equals(roomName));
		if(target == null)
			return null;
		
		/*Level fake = new Level{
			Session = new Session(l.Session.Area){
				Level = target.Name,
				Area = l.Session.Area
			}
		};
		if(fake.Session.MapData == null)
			throw new Exception("oh no");*/
		Session fakeSession = new Session(l.Session.Area){
			Level = target.Name
		};
		LevelLoader fakeLevelLoader = new LevelLoader(fakeSession, target.DefaultSpawn);
		new DynamicData(fakeLevelLoader).Invoke("LoadingThread_Safe");
		Level fake = fakeLevelLoader.Level;
		fake.LoadLevel(Player.IntroTypes.None, true);
		fake.Entities.UpdateLists();
		//fake.Update();

		/*var observers = fake.Tracker.GetEntities<Observer>();
		if(observers is not { Count: not (0 or >= 2) }){
			fake.UnloadLevel();
			return null; // throw?
		}*/

		return new WindowpaneManager(roomName, fake);
	}

	private WindowpaneManager(string roomName, Level scene) : base(scene){
		RoomName = roomName;
	}

	public override void Update(){
		base.Update();
		WrappedScene.Update();
	}

	public override void Removed(Scene scene){
		WrappedScene.UnloadLevel();
		base.Removed(scene);
	}

	public override void SceneEnd(Scene scene){
		WrappedScene.UnloadLevel();
		base.SceneEnd(scene);
	}

	public override void Render(){
		base.Render();
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
		
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
		//Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
		Renderer.BeforeRender(WrappedScene);
		Renderer.Render(WrappedScene);
		Renderer.AfterRender(WrappedScene);
		//Draw.SpriteBatch.End();
		
		Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Stencils.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
		Draw.SpriteBatch.Draw(Stencils.MaskRenderTarget, Vector2.Zero, Color.White);
		Draw.SpriteBatch.End();
		
		Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
		GameplayRenderer.Begin();
		Draw.SpriteBatch.Draw(Stencils.ObjectRenderTarget, camera.Position, Color.White);
		// GameplayRenderer.End();
		
	}
}