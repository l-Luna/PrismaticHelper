using System;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PrismaticHelper.Entities;

[Tracked]
public class CassetteListener : Component{

	public static readonly Color[] CassetteColours = {
		Calc.HexToColor("49aaf0"), Calc.HexToColor("f049be"), Calc.HexToColor("fcdc3a"), Calc.HexToColor("38e04e")
	};
	
	public Action<int> PreBeat, OnBeat, OnSilentBeat;
	public Action OnFinish;

	public CassetteListener() : base(false, false){}

	public static Color GetByIndex(int index){
		return CassetteColours[index < CassetteColours.Length ? index : 0];
	}

	public static void Load(){
		On.Celeste.CassetteBlockManager.SetWillActivate += CsPreActive;
		On.Celeste.CassetteBlockManager.SetActiveIndex += CsActive;
		On.Celeste.CassetteBlockManager.SilentUpdateBlocks += CsSilentActive;
		On.Celeste.CassetteBlockManager.StopBlocks += CsFinish;
	}

	public static void Unload(){
		On.Celeste.CassetteBlockManager.SetWillActivate -= CsPreActive;
		On.Celeste.CassetteBlockManager.SetActiveIndex -= CsActive;
		On.Celeste.CassetteBlockManager.SilentUpdateBlocks -= CsSilentActive;
		On.Celeste.CassetteBlockManager.StopBlocks -= CsFinish;
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

	private static void CsSilentActive(On.Celeste.CassetteBlockManager.orig_SilentUpdateBlocks orig, CassetteBlockManager self){
		orig(self);
		foreach(var listener in self.Scene.Tracker.GetComponents<CassetteListener>().Cast<CassetteListener>())
			listener.OnBeat?.Invoke(DynamicData.For(self).Get<int>("currentIndex"));
	}
	
	private static void CsFinish(On.Celeste.CassetteBlockManager.orig_StopBlocks orig, CassetteBlockManager self){
		orig(self);
		foreach(var listener in self.Scene.Tracker.GetComponents<CassetteListener>().Cast<CassetteListener>())
			listener.OnFinish?.Invoke();
	}
}