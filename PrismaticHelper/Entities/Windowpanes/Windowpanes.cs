using System.Collections.Generic;
using Celeste;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Windowpanes;

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
	
	public static bool IgnoreSessionStarts = false;
	
	public static void Load(){
		On.Celeste.SaveData.StartSession += SaveStartSession;
	}

	public static void Unload(){
		On.Celeste.SaveData.StartSession -= SaveStartSession;
		
		WindowpaneManager.Unload();
	}
	
	private static void SaveStartSession(On.Celeste.SaveData.orig_StartSession orig, SaveData self, Session session){
		if(!IgnoreSessionStarts)
			orig(self, session);
	}
}