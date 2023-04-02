using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Panels;

public static class WorldPanels{

	private const string worldPanelRoomNames = "PrismaticHelper:world_panel_rooms";
	private const string bgRoomNames = "PrismaticHelper:background_rooms";
	private const string noTintRoomNames = "PrismaticHelper:no_tint_rooms";

	public static void RegisterManagerRequired(Scene s, string room, bool background, bool noTint){
		List<string> req = RequiredManagers(s);
		if(!req.Contains(room))
			req.Add(room);
		DynamicData.For(s).Set(worldPanelRoomNames, req);

		List<string> bgs = BackgroundRooms(s);
		if(background && !bgs.Contains(room))
			bgs.Add(room);
		DynamicData.For(s).Set(bgRoomNames, bgs);
		
		List<string> nts = NoTintRooms(s);
		if(noTint && !nts.Contains(room))
			nts.Add(room);
		DynamicData.For(s).Set(noTintRoomNames, nts);
	}

	public static List<string> RequiredManagers(Scene s){
		return RoomsTagged(s, worldPanelRoomNames);
	}

	public static List<string> BackgroundRooms(Scene s){
		return RoomsTagged(s, bgRoomNames);
	}
	
	public static List<string> NoTintRooms(Scene s){
		return RoomsTagged(s, noTintRoomNames);
	}
	
	public static List<string> RoomsTagged(Scene s, string tag){
		return DynamicData.For(s).TryGet<List<string>>(tag, out var list) ? list : new List<string>();
	}

	public static void WpAwake(Scene s){
		var requiredManagers = RequiredManagers(s);
		var bgRooms = BackgroundRooms(s);
		var noTintRooms = NoTintRooms(s);
		foreach(var required in requiredManagers){
			var manager = WorldPanelManager.ofRoom(required, s, bgRooms.Contains(required), noTintRooms.Contains(required));
			if(manager != null)
				s.Add(manager);
		}
		requiredManagers.Clear();
		bgRooms.Clear();
		noTintRooms.Clear();
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

		WorldPanelManager.Unload();
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