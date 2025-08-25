using System;
using System.Collections.Generic;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using Renderite.Shared;
using static FrooxEngine.TrackerSettings;
using System.Runtime.InteropServices;
using SkyFrost.Base;
using System.Threading.Tasks;

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


	[HarmonyPatchCategory(nameof(RenderSpaceUpdate))]
	[HarmonyPatch(typeof(RenderSpaceUpdate), nameof(RenderSpaceUpdate.Pack))]
	public class FrameUpdateHandeler {
		public static bool Prefix(ref MemoryPacker packer, ref RenderSpaceUpdate __instance) {
			if (__instance.isOverlay) {
				return true;
			}

			sub(ref __instance.rootTransform.position, __instance.overridenViewTransform.position);
			__instance.overridenViewTransform.position = float3.Zero;

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
	[HarmonyPatch(typeof(RenderTransformManager), nameof(RenderTransformManager.EstimatedPoseUpdateCount), MethodType.Getter)]
	public class ResizeTransformBlock {
		static void Postfix(ref RenderTransformManager __instance, ref int __result) {
			__result += 1;
		}
	}

	//[HarmonyPatchCategory(nameof(WorldInitIntercept))]
	//[HarmonyPatch(typeof(World), MethodType.Constructor, new Type[] { typeof(WorldManager), typeof(bool), typeof(bool) })]
	//internal class WorldInitIntercept {
	//	public static void Postfix(World __instance) {
	//		Slot shaders = __instance.AssetsSlot.FindChildOrAdd("Shaders", true);
	//		Userspace u = new Userspace();
	//		u.RunSynchronously(() => {
	//			foreach (string url in Shaders.ChocoShaders) {
	//				Uri Uri = new Uri(url);
	//				StaticShader NewChocoShader = __instance.GetSharedComponentOrCreate(u.Cloud.Assets.DBSignature(Uri, false), delegate (StaticShader provider) {
	//					provider.URL.Value = Uri;
	//				}, 0, true, false, new Func<Slot>(() => { return shaders; }));
	//				NewChocoShader.Persistent = false;
	//				NewChocoShader.Asset.AssetId
	//			}
	//		});
	//	}
	//}

	[HarmonyPatchCategory(nameof(ShaderUpload))]
	[HarmonyPatch(typeof(ShaderUpload), nameof(ShaderUpload.Pack))]
	public class SubstituteShaders {
		static void Postfix(ref ShaderUpload __instance) {
			Msg("FrooxEngine is uploading " + __instance.file);
		}
	}

	[HarmonyPatchCategory(nameof(RenderTransformManager))]
	[HarmonyPatch(typeof(RenderTransformManager), "FillUpdate")]
	public class TransformMemoryBuilder {

		static Slot RootSlot;
		static bool WasUserspace;
		static int RootTransformIndex;
		static bool RootTransformDirty;

		private static void Prefix(Span<int> removals, Span<TransformParentUpdate> parentUpdates, Span<TransformPoseUpdate> poseUpdates, ref RenderTransformManager __instance) {
			if (__instance.World.IsUserspace()) {
				WasUserspace = true;
				return;
			}

			RootSlot = __instance.World.RootSlot;

			Traverse rootSlotTraverse = Traverse.Create(RootSlot);
			RootTransformIndex = rootSlotTraverse.Field("RenderTransformIndex").GetValue<int>();
			RootTransformDirty = rootSlotTraverse.Field("IsRenderTransformDirty").GetValue<bool>();
		}


		private static void Postfix(Span<int> removals, Span<TransformParentUpdate> parentUpdates, Span<TransformPoseUpdate> poseUpdates, ref RenderTransformManager __instance) {
			if (WasUserspace) {
				WasUserspace = false;
				return;
			}

			if (!RootTransformDirty) {
				TransformPoseUpdate rootPoseUpdate = new TransformPoseUpdate();

				rootPoseUpdate.transformId = RootTransformIndex;
				rootPoseUpdate.pose.position = RootSlot.LocalPosition;
				rootPoseUpdate.pose.rotation = RootSlot.LocalRotation;
				rootPoseUpdate.pose.scale = RootSlot.LocalScale;

				sub(ref rootPoseUpdate.pose.position, __instance.World.LocalUserViewTransform.position);

				poseUpdates[poseUpdates.Length - 1] = poseUpdates[0];
				poseUpdates[0] = rootPoseUpdate;

				return;
			}

			if (poseUpdates[RootTransformIndex].transformId == RootTransformIndex) {
				sub(ref poseUpdates[RootTransformIndex].pose.position, __instance.World.LocalUserViewTransform.position);
				return;
			}

			Error("Error while trying to modify Root Transform in DoublePrecision!");
			Error($"Expected to find RootTransform with index {RootTransformIndex}, found transform with index {poseUpdates[RootTransformIndex].transformId} instead!");
		}
	}

	public static void add(ref RenderVector3 a, RenderVector3 b) {
		a.x += b.x; a.y += b.y; a.z += b.z;
	}
	public static void sub(ref RenderVector3 a, RenderVector3 b) {
		a.x -= b.x; a.y -= b.y; a.z -= b.z;
	}
	public static void NegateVector(ref RenderVector3 t) {
		t.x *= -1;
		t.y *= -1;
		t.z *= -1;
	}

	[HarmonyPatch(typeof(PhotoCaptureManager), "GetCaptureCameraPosition")]
	class CaptureRepositioner {
		public static void Postfix(ref float3 pos, ref floatQ rot, ref PhotoCaptureManager __instance) {
			World world = __instance.World;
			Slot RootSlot = world.RootSlot;
			pos += RootSlot.LocalPosition - (float3)world.LocalUserViewTransform.position;
		}
	}

	[HarmonyPatch(typeof(MaterialProvider), "EnsureSharedShader")]
	class AnyShaderAnywherePatch {
		public static bool Prefix(AssetRef<Shader> assetRef, Uri url, MaterialProvider __instance, ref IAssetProvider<Shader> __result) {
			if (assetRef.Target == null)
				assetRef.Target = (IAssetProvider<Shader>)AccessTools.Method(typeof(MaterialProvider), "GetSharedShader").Invoke(__instance, new object[] { url });
			__result = assetRef.Target;
			return false;
		}
	}

	[HarmonyPatch(typeof(AssetInterface), "IsValidShader")]
	class AllShadersValidPatch {
		public static bool Prefix(ref Task<bool> __result) {
			__result = Task<bool>.Run(() => {
				return true;
			});
			return false;
		}
	}

	/*

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

	[HarmonyPatchCategory(nameof(TriplanarInitIntercept))]
	[HarmonyPatch(typeof(PBS_TriplanarMaterial), "InitializeSyncMembers")]
	internal class TriplanarInitIntercept {
		private static void Postfix(PBS_TriplanarMaterial __instance) {
			DataShare.FrooxMaterials.Add(__instance);
		}
	}
	*/
}
