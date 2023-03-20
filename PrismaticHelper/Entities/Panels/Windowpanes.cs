using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Panels;

public static class Windowpanes{
	
	private const string windowpaneRoomNames = "PrismaticHelper:windowpane_rooms";
	private const string bgRoomNames = "PrismaticHelper:background_rooms";

	public static void RegisterManagerRequired(Scene s, string room, bool background){
		List<string> req = RequiredManagers(s);
		if(!req.Contains(room))
			req.Add(room);
		DynamicData.For(s).Set(windowpaneRoomNames, req);

		List<string> bgs = BackgroundRooms(s);
		if(background && !bgs.Contains(room))
			bgs.Add(room);
		DynamicData.For(s).Set(bgRoomNames, bgs);
	}

	public static List<string> RequiredManagers(Scene s){
		return DynamicData.For(s).TryGet<List<string>>(windowpaneRoomNames, out var list) ? list : new List<string>();
	}
	
	public static List<string> BackgroundRooms(Scene s){
		return DynamicData.For(s).TryGet<List<string>>(bgRoomNames, out var list) ? list : new List<string>();
	}

	public static void WpAwake(Scene s){
		var requiredManagers = RequiredManagers(s);
		var bgRooms = BackgroundRooms(s);
		foreach(var required in requiredManagers){
			var manager = WindowpaneManager.ofRoom(required, s, bgRooms.Contains(required));
			if(manager != null)
				s.Add(manager);
		}
		requiredManagers.Clear();
	}
	
	public static bool ManipulateLevelLoads = false;
	
	public static void Load(){
		On.Celeste.SaveData.StartSession += SaveStartSession;
		On.Celeste.AudioState.Apply += AudioStateApply;
		On.Celeste.LevelLoader.ctor += LevelLoaderConstruct;
	}

	public static void Unload(){
		On.Celeste.SaveData.StartSession -= SaveStartSession;
		On.Celeste.AudioState.Apply -= AudioStateApply;
		On.Celeste.LevelLoader.ctor -= LevelLoaderConstruct;
		
		WindowpaneManager.Unload();
	}

	private static void AudioStateApply(On.Celeste.AudioState.orig_Apply orig, AudioState self){
		if(!ManipulateLevelLoads)
			orig(self);
	}

	private static void SaveStartSession(On.Celeste.SaveData.orig_StartSession orig, SaveData self, Session session){
		if(!ManipulateLevelLoads)
			orig(self, session);
	}
	
	private static void LevelLoaderConstruct(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startposition){
		if(ManipulateLevelLoads)
			self.orig_ctor(session, startposition);
		else
			orig(self, session, startposition);
	}
}