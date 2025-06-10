using System;
using System.Collections.Generic;
using UnityFrooxEngineRunner;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using UnityEngine;
using Elements.Core;
using System.Linq;

namespace ExampleMod;
//More info on creating mods can be found https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Creating-Mods
public class ExampleMod : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.4.0"; //Changing the version here updates it in all locations needed
	public override string Name => "DoublePrecision";
	public override string Author => "__Choco__";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/AwesomeTornado/Resonite-DoublePrecision-RML";

	public override void OnEngineInit() {
		Harmony harmony = new Harmony("com.example.DoublePrecision");
		harmony.PatchAll();
		Msg("DoublePrecision loaded.");
	}

	public class DataShare {
		public static List<World> frooxWorlds = new List<World>();
		public static List<GameObject> unityWorldRoots = new List<GameObject>();
		public static List<Vector3> FrooxCameraPosition = new List<Vector3>();
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
					DataShare.FrooxCameraPosition.Add(Vector3.zero);
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

		private static void Postfix(HeadOutput __instance) {
			int index = -1;
			for (int i = 0; i < DataShare.unityWorldRoots.Count; i++) {
				if (DataShare.frooxWorlds[i] is null || DataShare.frooxWorlds[i].IsDestroyed || DataShare.frooxWorlds[i].IsDisposed) {
					DataShare.frooxWorlds.RemoveAt(i);
					DataShare.unityWorldRoots.RemoveAt(i);
					DataShare.FrooxCameraPosition.RemoveAt(i);
				} else if (DataShare.frooxWorlds[i].Focus == World.WorldFocus.Focused) {
					index = i;
				}
			}
			if (index == -1) {
				Error("There are no valid focused worlds! Fatal error, exiting function.");
				return;
			}
			Vector3 playerMotion = __instance.transform.position - DataShare.FrooxCameraPosition[index];
			DataShare.FrooxCameraPosition[index] = __instance.transform.position;
			Vector3 pos = __instance.transform.position;
			Traverse instance_viewPos = Traverse.Create(__instance).Field("_viewPos");
			float3 newViewPos = instance_viewPos.GetValue<float3>() - new float3(pos.x, pos.y, pos.z);
			instance_viewPos.SetValue(newViewPos);
			//Do we really need viewScale?
			DataShare.unityWorldRoots[index].transform.position -= playerMotion;
			__instance.transform.position = Vector3.zero;
		}
	}
}
