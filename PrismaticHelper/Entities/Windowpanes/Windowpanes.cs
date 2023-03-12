using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Windowpanes;

public static class Windowpanes{
	
	private const string windowpaneRoomNames = "PrismaticHelper:windowpane_rooms";
	private const string needsControllers = "PrismaticHelper:needs_windowpane_controllers";

	public static void RegisterManagerRequired(Scene s, string room){
		DynamicData.For(s).Set(windowpaneRoomNames, new List<string>(RequiredManagers(s)){ room });
	}

	public static IEnumerable<string> RequiredManagers(Scene s){
		return DynamicData.For(s).TryGet<List<string>>(windowpaneRoomNames, out var list) ? list : new List<string>();
	}

	public static void WpAwake(Scene s){
		var sceneData = DynamicData.For(s);
		if(!sceneData.TryGet(needsControllers, out bool? b) || b == false){
			sceneData.Set(needsControllers, true);
			foreach(var required in RequiredManagers(s)){
				var manager = WindowpaneManager.ofRoom(required, s);
				if(manager != null)
					s.Add(manager);
			}
		}
	}

	public static void WpRemoved(Scene s){
		DynamicData.For(s).Set(needsControllers, true);
	}

	public static VirtualRenderTarget CurrentSubstitute = null;
	public static bool IgnoreSessionStarts = false;
	
	public static void Load(){
		IL.Celeste.Level.Render += LevelOnRender;
		On.Celeste.SaveData.StartSession += SaveStartSession;
	}

	public static void Unload(){
		IL.Celeste.Level.Render -= LevelOnRender;
		On.Celeste.SaveData.StartSession -= SaveStartSession;
	}
	
	private static void LevelOnRender(ILContext il){
		ILCursor cursor = new ILCursor(il);
		// IL_01fb: ldnull
		// IL_01fc: callvirt     instance void [FNA]Microsoft.Xna.Framework.Graphics.GraphicsDevice::SetRenderTarget(class [FNA]Microsoft.Xna.Framework.Graphics.RenderTarget2D)
		if(cursor.TryGotoNext(MoveType.After,
			   instr => instr.MatchLdnull(),
			   instr => instr.MatchCallvirt<GraphicsDevice>("SetRenderTarget"))){
			PrismaticHelperModule.LogInfo("Hooking into Level.Render for Prismatic Helper windowpanes!");
			cursor.EmitDelegate(() => {
				if(CurrentSubstitute != null)
					Engine.Graphics.GraphicsDevice.SetRenderTarget(CurrentSubstitute);
				else
					Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
			});
		}
	}
	
	private static void SaveStartSession(On.Celeste.SaveData.orig_StartSession orig, SaveData self, Session session){
		if(!IgnoreSessionStarts)
			orig(self, session);
	}
}