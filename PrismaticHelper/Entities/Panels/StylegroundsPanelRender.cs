using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace PrismaticHelper.Entities.Panels;

// third time's the charm
public static class StylegroundsPanelRenderer{

	public static void UpdateStylegroundPanels(bool fg, Scene level, BackdropRenderer renderer){
		List<IGrouping<string, AbstractPanel>> toUpdate = level.Tracker
			.GetEntities<StylegroundsPanel>()
			.Cast<AbstractPanel>()
			.Where(it => it.Foreground == fg)
			.Concat(level.Tracker.GetEntities<Windowpane>().Cast<AbstractPanel>())
			.GroupBy(it => it.RoomName)
			.ToList();

		foreach(var item in toUpdate){
			var room = item.Key;
			Level lvl = level as Level;
			foreach(Backdrop bg in renderer.Backdrops)
				if(IsVisible(bg, lvl, room) && !bg.Visible){
					bool wasVisible = bg.Visible, wasForceVisible = bg.ForceVisible;
					bg.Visible = true;
					bg.ForceVisible = true;
					// TODO: also check if visible in any rooms that are part of a room transition
					if(!lvl.Paused && !IsVisible(bg, lvl, lvl.Session.Level, !wasForceVisible))
						bg.Update(level);
					bg.BeforeRender(level);
					bg.Visible = wasVisible;
					bg.ForceVisible = wasForceVisible;
				}
		}
	}

	public static void RenderStylegroundsPanels(bool fg, Scene level, BackdropRenderer renderer){

		Camera camera = (level as Level).Camera;
		// find all of the panels we want to fill
		List<IGrouping<string, StylegroundsPanel>> toRender = level.Entities
			.FindAll<StylegroundsPanel>()
			.Where(it => it.Foreground == fg)
			.Where(it => it.VisibleOnScreen(camera))
			.GroupBy(it => it.RoomName)
			.ToList();

		foreach(var item in toRender){
			var room = item.Key;
			Level lvl = level as Level;
			// get our styleground mask
			Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.MaskRenderTarget);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin();
			foreach(var panel in item)
				panel.DrawMask(camera);
			GameplayRenderer.End();
			// render some styleground
			Engine.Graphics.GraphicsDevice.SetRenderTarget(Stencils.ObjectRenderTarget);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			RenderStylegroundsForRoom(room, renderer, lvl);
			// apply the mask to the styleground
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Stencils.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
			Draw.SpriteBatch.Draw(Stencils.MaskRenderTarget, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();
			// render the styleground
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
			GameplayRenderer.Begin();
			Draw.SpriteBatch.Draw(Stencils.ObjectRenderTarget, camera.Position, Color.White);
			GameplayRenderer.End();
		}
	}

	public static void RenderStylegroundsForRoom(string room, BackdropRenderer renderer, Level level){
		BlendState blendState = BlendState.AlphaBlend;
		bool usingSpritebatch = false;
		foreach(Backdrop backdrop in renderer.Backdrops){
			if(IsVisible(backdrop, level, room)){
				if(backdrop is Parallax p && p.BlendState != blendState){
					if(usingSpritebatch)
						Draw.SpriteBatch.End();
					usingSpritebatch = false;
					blendState = p.BlendState;
				}

				if(backdrop.UseSpritebatch && !usingSpritebatch){
					Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
					usingSpritebatch = true;
				}

				if(!backdrop.UseSpritebatch && usingSpritebatch){
					if(usingSpritebatch)
						Draw.SpriteBatch.End();
					usingSpritebatch = false;
				}

				backdrop.Render(level);
			}
		}

		if(usingSpritebatch)
			Draw.SpriteBatch.End();
	}

	public static void Load(){
		On.Celeste.BackdropRenderer.Render += OnBackdropRendererRender;
		On.Celeste.BackdropRenderer.Update += OnBackdropRendererUpdate;
	}

	public static void Unload(){
		On.Celeste.BackdropRenderer.Render -= OnBackdropRendererRender;
		On.Celeste.BackdropRenderer.Update -= OnBackdropRendererUpdate;
	}

	public static void OnBackdropRendererRender(On.Celeste.BackdropRenderer.orig_Render orig, BackdropRenderer self, Scene scene){
		orig(self, scene);
		if(scene is Level level)
			if(self == level.Foreground)
				RenderStylegroundsPanels(true, level, self);
			else if(self == level.Background)
				RenderStylegroundsPanels(false, level, self);
	}

	public static void OnBackdropRendererUpdate(On.Celeste.BackdropRenderer.orig_Update orig, BackdropRenderer self, Scene scene){
		orig(self, scene);
		if(scene is Level level)
			if(self == level.Foreground)
				UpdateStylegroundPanels(true, level, self);
			else if(self == level.Background)
				UpdateStylegroundPanels(false, level, self);
	}

	private static bool IsVisible(Backdrop styleground, Level level, string room, bool ignoreFV = false){
		if(!ignoreFV && styleground.ForceVisible)
			return true;

		if(!string.IsNullOrEmpty(styleground.OnlyIfNotFlag) && level.Session.GetFlag(styleground.OnlyIfNotFlag))
			return false;

		if(!string.IsNullOrEmpty(styleground.AlsoIfFlag) && level.Session.GetFlag(styleground.AlsoIfFlag))
			return true;

		if(styleground.Dreaming.HasValue && styleground.Dreaming.Value != level.Session.Dreaming)
			return false;

		if(!string.IsNullOrEmpty(styleground.OnlyIfFlag) && !level.Session.GetFlag(styleground.OnlyIfFlag))
			return false;

		if(styleground.ExcludeFrom != null && styleground.ExcludeFrom.Contains(room))
			return false;

		if(styleground.OnlyIn != null && !styleground.OnlyIn.Contains(room))
			return false;

		return true;
	}
}