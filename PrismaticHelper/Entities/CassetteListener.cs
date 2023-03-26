using System;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities;

[Tracked]
public class CassetteListener : Component{

	public static readonly Color[] CassetteColours = {
		Calc.HexToColor("49aaf0"), Calc.HexToColor("f049be"), Calc.HexToColor("fcdc3a"), Calc.HexToColor("38e04e")
	};
	
	public Action<int> PreBeat;
	public Action<int> OnBeat;

	public CassetteListener() : base(false, false){}

	public static Color GetByIndex(int index){
		return CassetteColours[index < CassetteColours.Length ? index : 0];
	}

	public static void Load(){
		On.Celeste.CassetteBlockManager.SetWillActivate += CsPreActive;
		On.Celeste.CassetteBlockManager.SetActiveIndex += CsActive;
	}

	public static void Unload(){
		On.Celeste.CassetteBlockManager.SetWillActivate -= CsPreActive;
		On.Celeste.CassetteBlockManager.SetActiveIndex -= CsActive;
	}
	
	private static void CsPreActive(On.Celeste.CassetteBlockManager.orig_SetWillActivate orig, CassetteBlockManager self, int i){
		orig(self, i);
		foreach(var listener in self.Scene.Tracker.GetComponents<CassetteListener>().Cast<CassetteListener>())
			listener.PreBeat?.Invoke(i);
	}
	
	private static void CsActive(On.Celeste.CassetteBlockManager.orig_SetActiveIndex orig, CassetteBlockManager self, int i){
		orig(self, i);
		foreach(var listener in self.Scene.Tracker.GetComponents<CassetteListener>().Cast<CassetteListener>())
			listener.OnBeat?.Invoke(i);
	}
}