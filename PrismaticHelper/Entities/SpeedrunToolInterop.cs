using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Monocle;

namespace PrismaticHelper.Entities;

public static class SpeedrunToolInterop{

	private const string islcName = "Celeste.Mod.SpeedrunTool.SaveLoad.IgnoreSaveLoadComponent";
	private const string slaName = "Celeste.Mod.SpeedrunTool.SaveLoad.SaveLoadAction";

	private static readonly EverestModuleMetadata speedrunToolRequired = new(){
		Name = "SpeedrunTool",
		VersionString = "3.0.0"
	};
	
	public static Component TryCreateIgnoreSavestateComponent(){
		if(Everest.Loader.TryGetDependency(speedrunToolRequired, out var module))
			return (Component)module.GetType().Assembly.GetType(islcName)?.GetConstructor(Type.EmptyTypes)?.Invoke(new object[0]);
		return null;
	}

	public static void NonSavestatable(Entity e){
		var iscl = TryCreateIgnoreSavestateComponent();
		if(iscl != null)
			e.Add(iscl);
	}

	public static void AddPostSavestateLoadAction(Action<Level> action){
		Action<Dictionary<Type, Dictionary<string, object>>, Level> loadAction = (_, level) => action(level);
		if(Everest.Loader.TryGetDependency(speedrunToolRequired, out var module))
			module.GetType().Assembly.GetType("slaName")?.GetMethod("SafeAdd")?.Invoke(null, new object[]{ loadAction, loadAction, null, null, null });
	}
}