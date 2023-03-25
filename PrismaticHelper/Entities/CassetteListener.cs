using System;
using System.Linq;
using Celeste;
using Monocle;

namespace PrismaticHelper.Entities;

[Tracked]
public class CassetteListener : Component{

	public Action<int> OnBeat;

	public CassetteListener() : base(false, false){}
	
	public static void Load(){
		On.Celeste.CassetteBlockManager.SetActiveIndex += CsActive;
	}

	public static void Unload(){
		On.Celeste.CassetteBlockManager.SetActiveIndex -= CsActive;
	}
	
	private static void CsActive(On.Celeste.CassetteBlockManager.orig_SetActiveIndex orig, CassetteBlockManager self, int i){
		orig(self, i);
		foreach(var listener in self.Scene.Tracker.GetComponents<CassetteListener>().Cast<CassetteListener>())
			listener.OnBeat?.Invoke(i);
	}
}