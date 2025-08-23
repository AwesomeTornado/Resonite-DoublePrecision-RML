using System;
using System.Collections.Generic;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using Renderite.Shared;
using static FrooxEngine.TrackerSettings;
using UnityEngine;
using System.Runtime.InteropServices;

namespace DoublePrecision;
//More info on creating mods can be found https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Creating-Mods
public class DoublePrecision : ResoniteMod {
	internal const string VERSION_CONSTANT = "2.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "DoublePrecision";
	public override string Author => "__Choco__";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/AwesomeTornado/Resonite-DoublePrecision-RML";

	public override void OnEngineInit() {
		Harmony harmony = new Harmony("com.__Choco__.DoublePrecision");
		harmony.PatchAll();
		Msg("DoublePrecision loaded.");
	}

	static RenderVector3 ScreenCameraPosition = new RenderVector3();
	static readonly RenderVector3 RenderVectorZero = new RenderVector3();

	[HarmonyPatchCategory(nameof(RenderSpaceUpdate))]
	[HarmonyPatch(typeof(RenderSpaceUpdate), nameof(RenderSpaceUpdate.Pack))]
	public class FrameUpdateHandeler {
		public static bool Prefix(ref MemoryPacker packer, ref RenderSpaceUpdate __instance) {
			ScreenCameraPosition = __instance.overridenViewTransform.position;
			//__instance.overridenViewTransform.position = RenderVectorZero;
			__instance.rootTransform.position = RenderVectorZero;
			packer.Write<int>(__instance.id);
			packer.Write<bool>(__instance.isActive);
			packer.Write<bool>(__instance.isOverlay);
			packer.Write<bool>(__instance.isPrivate);
			packer.Write<RenderTransform>(__instance.rootTransform);
			packer.Write<bool>(__instance.viewPositionIsExternal);
			packer.Write<bool>(__instance.overrideViewPosition);
			packer.Write<int>(__instance.skyboxMaterialAssetId);
			packer.Write<RenderSH2>(__instance.ambientLight);
			packer.Write<RenderTransform>(__instance.overridenViewTransform);
			packer.WriteObject<TransformsUpdate>(__instance.transformsUpdate);
			packer.WriteObject<MeshRenderablesUpdate>(__instance.meshRenderersUpdate);
			packer.WriteObject<SkinnedMeshRenderablesUpdate>(__instance.skinnedMeshRenderersUpdate);
			packer.WriteObject<LightRenderablesUpdate>(__instance.lightsUpdate);
			packer.WriteObject<CameraRenderablesUpdate>(__instance.camerasUpdate);
			packer.WriteObject<CameraPortalsRenderablesUpdate>(__instance.cameraPortalsUpdate);
			packer.WriteObject<ReflectionProbeRenderablesUpdate>(__instance.reflectionProbesUpdate);
			packer.WriteObject<ReflectionProbeSH2Tasks>(__instance.reflectionProbeSH2Taks);
			packer.WriteObject<LayerUpdate>(__instance.layersUpdate);
			packer.WriteObject<BillboardRenderBufferUpdate>(__instance.billboardBuffersUpdate);
			packer.WriteObject<MeshRenderBufferUpdate>(__instance.meshRenderBuffersUpdate);
			packer.WriteObject<TrailsRendererUpdate>(__instance.trailRenderersUpdate);
			packer.WriteObject<LightsBufferRendererUpdate>(__instance.lightsBufferRenderersUpdate);
			packer.WriteObject<RenderTransformOverridesUpdate>(__instance.renderTransformOverridesUpdate);
			packer.WriteObject<RenderMaterialOverridesUpdate>(__instance.renderMaterialOverridesUpdate);
			packer.WriteObject<BlitToDisplayRenderablesUpdate>(__instance.blitToDisplaysUpdate);
			packer.WriteObject<LODGroupRenderablesUpdate>(__instance.lodGroupUpdate);
			packer.WriteObject<GaussianSplatRenderablesUpdate>(__instance.gaussianSplatRenderersUpdate);
			packer.WriteObjectList<ReflectionProbeRenderTask>(__instance.reflectionProbeRenderTasks);

			return false;
		}
	}

	[HarmonyPatchCategory(nameof(RenderTransformManager))]
	[HarmonyPatch(typeof(RenderTransformManager), "FillUpdate")]
	public class TransformMemoryBuilder {
		private static void Postfix(Span<int> removals, Span<TransformParentUpdate> parentUpdates, Span<TransformPoseUpdate> poseUpdates, ref RenderTransformManager __instance) {
			Engine engine = Engine.Current;
			World? focusedWorld = engine.WorldManager.FocusedWorld;
			bool? isAllocated = focusedWorld?.RootSlot.IsRenderTransformAllocated;
			if (focusedWorld == null)
				return;
			//for (int i = 0; i < poseUpdates.Length; i++) {
			//	RenderVector3 pos = poseUpdates[i].pose.position;
			//	pos = new RenderVector3(1+pos.x, 1+pos.y, 1+pos.z);
			//	poseUpdates[i].pose.position = pos;
			//}
			if (poseUpdates[0].transformId == 0) {
				RenderVector3 pos = poseUpdates[0].pose.position;
				pos = sub(pos, ScreenCameraPosition);
				poseUpdates[0].pose.position = pos;
			}
			Traverse rootSlotTraverse = Traverse.Create(focusedWorld.RootSlot);
			int renderIndex = rootSlotTraverse.Field("RenderTransformIndex").GetValue<int>();
			Msg($"Hooke works!, RootAllocated {isAllocated}, with index {renderIndex}");
		}
	}

	public static RenderVector3 add(RenderVector3 a, RenderVector3 b) {
		return new RenderVector3(a.x + b.x, a.y + b.y, a.z + b.z);
		//could make this slightly more efficient by using ref and directly writing to a
	}
	public static RenderVector3 sub(RenderVector3 a, RenderVector3 b) {
		return new RenderVector3(a.x - b.x, a.y - b.y, a.z - b.z);
		//could make this slightly more efficient by using ref and directly writing to a
	}






	/*
	public class DataShare {
		public static List<World> frooxWorlds = new List<World>();
		public static List<GameObject> unityWorldRoots = new List<GameObject>();
		public static List<Vector3> FrooxCameraPosition = new List<Vector3>();
		public static List<PBS_TriplanarMaterial> FrooxMaterials = new List<PBS_TriplanarMaterial>();

		public static int FocusedWorld() {
			int index = -1;
			for (int i = 0; i < DataShare.unityWorldRoots.Count; i++) {
				if (DataShare.frooxWorlds[i] is null || DataShare.frooxWorlds[i].IsDestroyed || DataShare.frooxWorlds[i].IsDisposed) {
					DataShare.frooxWorlds.RemoveAt(i);
					DataShare.unityWorldRoots.RemoveAt(i);
					DataShare.FrooxCameraPosition.RemoveAt(i);
					i--;
				} else if (DataShare.frooxWorlds[i].Focus == World.WorldFocus.Focused) {
					index = i;
				}
			}
			if (index == -1) {
				World w = Userspace.UserspaceWorld;
				var worldsList = w.WorldManager.Worlds;
				foreach (World world in worldsList) {
					WorldInitIntercept.Postfix(world);
				}
				return -1;
			}
			return index;
		}
	}

	[HarmonyPatchCategory(nameof(WorldInitIntercept))]
	[HarmonyPatch(typeof(World), MethodType.Constructor, new Type[] { typeof(WorldManager), typeof(bool), typeof(bool) })]
	internal class WorldInitIntercept {
		public static void Postfix(World __instance) {
			WorldConnector? worldConnector = __instance.Connector as WorldConnector;
			if (worldConnector is not null) {
				DataShare.frooxWorlds.Add(__instance);
				DataShare.unityWorldRoots.Add(worldConnector.WorldRoot);
				DataShare.FrooxCameraPosition.Add(Vector3.zero);
			} else {
				Error("Unable to cast IWorldConnector to WorldConnector.");
			}
		}
	}

	[HarmonyPatchCategory(nameof(Camera_Patches))]
	[HarmonyPatch(typeof(HeadOutput), nameof(HeadOutput.UpdatePositioning))]
	internal class Camera_Patches {
		private static void Postfix(HeadOutput __instance) {
			int index = DataShare.FocusedWorld();
			if (index == -1) return;

			Vector3 playerMotion = __instance.transform.position - DataShare.FrooxCameraPosition[index];
			DataShare.FrooxCameraPosition[index] = __instance.transform.position;
			Vector3 pos = __instance.transform.position;
			Traverse instance_viewPos = Traverse.Create(__instance).Field("_viewPos");
			float3 newViewPos = instance_viewPos.GetValue<float3>() - new float3(pos.x, pos.y, pos.z);
			instance_viewPos.SetValue(newViewPos);
			//Do we really need viewScale?
			DataShare.unityWorldRoots[index].transform.position -= playerMotion;
			__instance.transform.position = Vector3.zero;
			Vector3 rootPos = DataShare.unityWorldRoots[index].transform.position;
			for (int i = 0; i < DataShare.FrooxMaterials.Count; i++) {
				if (DataShare.FrooxMaterials[i] is not null) {
					if (DataShare.FrooxMaterials[i].Asset is not null) {
						DataShare.FrooxMaterials[i].Asset.GetUnity().SetVector("_WorldOffset", rootPos);
					}
				}
				//TODO: Add cache invalidation for FrooxMaterials.
			}
		}
	}

	[HarmonyPatchCategory(nameof(PBS_Tri_Metal_Overhaul))]
	internal class PBS_Tri_Metal_Overhaul {

		[HarmonyPatch(typeof(PBS_TriplanarMetallic), "GetShader")]
		private static bool Prefix(PBS_TriplanarMetallic __instance, ref FrooxEngine.Shader __result) {
			Uri URL;
			Uri officialURL;
			AssetRef<FrooxEngine.Shader> instance_shader;
			if (__instance.Transparent) {
				URL = new Uri(Shaders.resdb_choco_transparent);
				officialURL = new Uri(Shaders.resdb_froox_transparent);
				instance_shader = Traverse.Create(__instance).Field("_transparent").GetValue<AssetRef<FrooxEngine.Shader>>();
			} else {
				URL = new Uri(Shaders.resdb_choco);
				officialURL = new Uri(Shaders.resdb_froox);
				instance_shader = Traverse.Create(__instance).Field("_regular").GetValue<AssetRef<FrooxEngine.Shader>>();
			}
			__result = Traverse.Create(__instance).Method("EnsureSharedShader", new object[] { instance_shader, officialURL }).GetValue<IAssetProvider<FrooxEngine.Shader>>().Asset;
			if (((StaticShader)instance_shader.Target).URL.Value != URL) {
				World w = __instance.World;
				var Override = ValueUserOverride.OverrideForUser(((StaticShader)instance_shader.Target).URL, w.LocalUser, URL);
				Override.Default.Value = officialURL;
				Override.Persistent = false;
			}
			return false; //never run original function
		}
	}

	[HarmonyPatchCategory(nameof(PBS_Tri_Specular_Overhaul))]
	internal class PBS_Tri_Specular_Overhaul {

		[HarmonyPatch(typeof(PBS_TriplanarSpecular), "GetShader")]
		private static bool Prefix(PBS_TriplanarSpecular __instance, ref FrooxEngine.Shader __result) {
			Uri URL;
			Uri officialURL;
			AssetRef<FrooxEngine.Shader> instance_shader;
			if (__instance.Transparent) {
				URL = new Uri(Shaders.resdb_choco_transparent_specular);
				officialURL = new Uri(Shaders.resdb_froox_transparent_specular);
				instance_shader = Traverse.Create(__instance).Field("_transparent").GetValue<AssetRef<FrooxEngine.Shader>>();
			} else {
				URL = new Uri(Shaders.resdb_choco_specular);
				officialURL = new Uri(Shaders.resdb_froox_specular);
				instance_shader = Traverse.Create(__instance).Field("_regular").GetValue<AssetRef<FrooxEngine.Shader>>();
			}
			__result = Traverse.Create(__instance).Method("EnsureSharedShader", new object[] { instance_shader, officialURL }).GetValue<IAssetProvider<FrooxEngine.Shader>>().Asset;
			if (((StaticShader)instance_shader.Target).URL.Value != URL) {
				World w = __instance.World;
				var Override = ValueUserOverride.OverrideForUser(((StaticShader)instance_shader.Target).URL, w.LocalUser, URL);
				Override.Default.Value = officialURL;
				Override.Persistent = false;
			}
			return false; //never run original function
		}
	}

	[HarmonyPatchCategory(nameof(RenderingRepositioning))]
	[HarmonyPatch(typeof(RenderConnector), nameof(RenderConnector.RenderImmediate))]
	internal class RenderingRepositioning {
		private static void Prefix(RenderConnector __instance, global::FrooxEngine.RenderSettings renderSettings) {
			int index = DataShare.FocusedWorld();
			if (index == -1) return;
			Vector3 xyz = DataShare.unityWorldRoots[index].transform.position;
			renderSettings.position += new float3(xyz.x, xyz.y, xyz.z);
		}
	}

	[HarmonyPatchCategory(nameof(TriplanarInitIntercept))]
	[HarmonyPatch(typeof(PBS_TriplanarMaterial), "InitializeSyncMembers")]
	internal class TriplanarInitIntercept {
		private static void Postfix(PBS_TriplanarMaterial __instance) {
			DataShare.FrooxMaterials.Add(__instance);
		}
	}
	*/
}
