using System;
using System.Collections.Generic;
using UnityFrooxEngineRunner;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using UnityEngine;
using System.Linq;

namespace ExampleMod;
//More info on creating mods can be found https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Creating-Mods
public class ExampleMod : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.3.0"; //Changing the version here updates it in all locations needed
	public override string Name => "DoublePrecision";
	public override string Author => "__Choco__";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/AwesomeTornado/Resonite-DoublePrecision-RML";

	public override void OnEngineInit() {
		Harmony harmony = new Harmony("com.example.DoublePrecision");
		harmony.PatchAll();
	}

	public class DataShare {
		public static List<World> frooxWorlds = new List<World>();
		public static List<GameObject> unityWorldRoots = new List<GameObject>();
		public static List<Vector3> worldOffset = new List<Vector3>();
		//public static Vector3 FrooxEngineCameraPosition = Vector3.zero; //this may need to be added back in as a list if there are offset problems when switching worlds while moving.
	}

	//Example of how a HarmonyPatch can be formatted, Note that the following isn't a real patch and will not compile.
	[HarmonyPatch(typeof(World), MethodType.Constructor, new Type[] { typeof(WorldManager), typeof(bool), typeof(bool) })]
	class WorldConstructor_CacheWorlds {

		private static bool IsUserspaceInitialized = false;

		private static void Postfix(World __instance) {
			if (IsUserspaceInitialized) {
				WorldConnector? worldConnector = __instance.Connector as WorldConnector;
				if (worldConnector is not null) {
					DataShare.frooxWorlds.Add(__instance);
					DataShare.unityWorldRoots.Add(worldConnector.WorldRoot);
					DataShare.worldOffset.Add(Vector3.zero);
					//possibly add in FrooxEngineCameraPosition list init here if needed.
				} else {
					Error("Unable to cast IWorldConnector to WorldConnector.");
				}
			} else {
				Msg("First init, assuming this world is Userspace, and skipping.");
				IsUserspaceInitialized = true;
			}
		}
	}


	[HarmonyPatch(typeof(HeadOutput), nameof(HeadOutput.UpdatePositioning))]
	class Camera_Patches {

		private static Vector3 FrooxEngineCameraPosition = Vector3.zero;

		private static HeadOutput.HeadOutputType? prevOutputMode = null;

		private static void Postfix(HeadOutput __instance) {
			int index = -1;
			for (int i = 0; i < DataShare.unityWorldRoots.Count; i++) {
				if (DataShare.frooxWorlds[i] is null || DataShare.frooxWorlds[i].IsDestroyed || DataShare.frooxWorlds[i].IsDisposed) {
					DataShare.frooxWorlds.RemoveAt(i);
					DataShare.unityWorldRoots.RemoveAt(i);
				} else if (DataShare.frooxWorlds[i].Focus == World.WorldFocus.Focused) {
					index = i;
				}
			}
			if (index == -1) {
				Error("There are no valid focused worlds! Fatal error, exiting function.");
				return;
			}
			if (prevOutputMode is null) {
				prevOutputMode = __instance.Type;
			}
			Vector3 playerMotion = playerMotion = __instance.transform.position - FrooxEngineCameraPosition;
			FrooxEngineCameraPosition = __instance.transform.position;
			switch (__instance.Type) {
				case HeadOutput.HeadOutputType.VR: {
						if (prevOutputMode != HeadOutput.HeadOutputType.VR) {
							prevOutputMode = HeadOutput.HeadOutputType.VR;//reset prev output mode
							DataShare.unityWorldRoots[index].transform.position = DataShare.worldOffset[index];
							DataShare.worldOffset[index] = Vector3.zero;
						}
						DataShare.unityWorldRoots[index].transform.position -= playerMotion;
						break;
					}
				case HeadOutput.HeadOutputType.Screen: {
						if (prevOutputMode != HeadOutput.HeadOutputType.Screen) {
							prevOutputMode = HeadOutput.HeadOutputType.Screen;//reset prev output mode
							DataShare.worldOffset[index] = DataShare.unityWorldRoots[index].transform.position;
							DataShare.unityWorldRoots[index].transform.position = Vector3.zero;
						}
						DataShare.worldOffset[index] -= playerMotion;//move this to record where the world *should* be, instead of moving the world.
						break;
					}
			}
			prevOutputMode = __instance.Type;
			__instance.transform.position = Vector3.zero;
		}
	}
}
