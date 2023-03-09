using System.Collections.Generic;
using Monocle;
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

	public static void Awake(Scene s){
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
}