using System.Collections.Generic;
using Celeste;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities.Windowpanes;

public static class Windowpanes{
	
	private const string windowpaneRoomNames = "PrismaticHelper:windowpane_rooms";

	public static void RegisterManagerRequired(Scene s, string room){
		DynamicData.For(s).Set(windowpaneRoomNames, new List<string>(RequiredManagers(s)){ room });
	}

	public static List<string> RequiredManagers(Scene s){
		return DynamicData.For(s).TryGet<List<string>>(windowpaneRoomNames, out var list) ? list : new List<string>();
	}

	public static void WpAwake(Scene s){
		var requiredManagers = RequiredManagers(s);
		foreach(var required in requiredManagers){
			var manager = WindowpaneManager.ofRoom(required, s);
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
	}
	
	private static void SaveStartSession(On.Celeste.SaveData.orig_StartSession orig, SaveData self, Session session){
		if(!IgnoreSessionStarts)
			orig(self, session);
	}
}