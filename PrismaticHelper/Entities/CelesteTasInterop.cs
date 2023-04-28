using System;
using System.Reflection;
using Celeste.Mod;

namespace PrismaticHelper.Entities;

// Based on Frost Helper, in turn based on Communal Helper
public static class CelesteTasInterop{
	
	private static MethodInfo CelesteTAS_PlayerStates_Register;
	private static MethodInfo CelesteTAS_PlayerStates_Unregister;
	
	public static void Load(){
		EverestModuleMetadata celesteTASMeta = new EverestModuleMetadata{ Name = "CelesteTAS", VersionString = "3.4.5" };
		if(Everest.Loader.TryGetDependency(celesteTASMeta, out EverestModule tasModule)){
			Type playerStatesType = tasModule.GetType().Module.GetType("TAS.PlayerStates");
			CelesteTAS_PlayerStates_Register = playerStatesType.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
			CelesteTAS_PlayerStates_Unregister = playerStatesType.GetMethod("Unregister", BindingFlags.Public | BindingFlags.Static);
		}
	}

	public static void RegisterState(int state, string stateName)
		=> CelesteTAS_PlayerStates_Register?.Invoke(null, new object[]{ state, stateName });

	public static void UnregisterState(int state)
		=> CelesteTAS_PlayerStates_Unregister?.Invoke(null, new object[]{ state });
}