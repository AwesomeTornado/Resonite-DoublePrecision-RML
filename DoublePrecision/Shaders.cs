using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FrooxEngine;

namespace DoublePrecision;
static class Shaders {
	public const string choco = "e907ed0ca29b3534896947c4ea0004dbfa8baae96a645f3539a9516f3e9d369f";
	public const string choco_specular = "8500b5e85587ab83a88f1dec000bbbe8a3fc760ede5c1df4242a3b6372273d27";
	public const string choco_transparent = "f3d267a56478c4a756e5d3f71195fa68bb091c9486a48d136aa23ca27d042a35";
	public const string choco_transparent_specular = "121e7a5a66c70a278e99adeeb2e7ee7090c00064f3d6312cdd5026892f56023b";

	public const string froox = "d9d43057b97ff2e71b9947af38c754cde992bfa8ea75ab34d9e43859e0a0f7d3";
	public const string froox_specular = "33dcd39d588e92840eb58845d3a0404e75c038e277a92b634721dcecc16dfd9a";
	public const string froox_transparent = "abfef7119e75779d7ad31222211acc4dbcced41bacda4fd32bf04fac633c8b1b";
	public const string froox_transparent_specular = "205b3cc9c239927986895a41e6cc7323853da09b87d23c5b466a2940b4e4de92";

	public const string ext_choco = "e907ed0ca29b3534896947c4ea0004dbfa8baae96a645f3539a9516f3e9d369f.__Choco__Shader!";
	public const string ext_choco_specular = "8500b5e85587ab83a88f1dec000bbbe8a3fc760ede5c1df4242a3b6372273d27.__Choco__Shader!";
	public const string ext_choco_transparent = "f3d267a56478c4a756e5d3f71195fa68bb091c9486a48d136aa23ca27d042a35.__Choco__Shader!";
	public const string ext_choco_transparent_specular = "121e7a5a66c70a278e99adeeb2e7ee7090c00064f3d6312cdd5026892f56023b.__Choco__Shader!";

	public const string ext_froox = "d9d43057b97ff2e71b9947af38c754cde992bfa8ea75ab34d9e43859e0a0f7d3.unityshader";
	public const string ext_froox_specular = "33dcd39d588e92840eb58845d3a0404e75c038e277a92b634721dcecc16dfd9a.unityshader";
	public const string ext_froox_transparent = "abfef7119e75779d7ad31222211acc4dbcced41bacda4fd32bf04fac633c8b1b.unityshader";
	public const string ext_froox_transparent_specular = "205b3cc9c239927986895a41e6cc7323853da09b87d23c5b466a2940b4e4de92.unityshader";

	public const string resdb_choco = "resdb:///e907ed0ca29b3534896947c4ea0004dbfa8baae96a645f3539a9516f3e9d369f.__Choco__Shader!";
	public const string resdb_choco_specular = "resdb:///8500b5e85587ab83a88f1dec000bbbe8a3fc760ede5c1df4242a3b6372273d27.__Choco__Shader!";
	public const string resdb_choco_transparent = "resdb:///f3d267a56478c4a756e5d3f71195fa68bb091c9486a48d136aa23ca27d042a35.__Choco__Shader!";
	public const string resdb_choco_transparent_specular = "resdb:///121e7a5a66c70a278e99adeeb2e7ee7090c00064f3d6312cdd5026892f56023b.__Choco__Shader!";

	public const string resdb_froox = "resdb:///d9d43057b97ff2e71b9947af38c754cde992bfa8ea75ab34d9e43859e0a0f7d3.unityshader";
	public const string resdb_froox_specular = "resdb:///33dcd39d588e92840eb58845d3a0404e75c038e277a92b634721dcecc16dfd9a.unityshader";
	public const string resdb_froox_transparent = "resdb:///abfef7119e75779d7ad31222211acc4dbcced41bacda4fd32bf04fac633c8b1b.unityshader";
	public const string resdb_froox_transparent_specular = "resdb:///205b3cc9c239927986895a41e6cc7323853da09b87d23c5b466a2940b4e4de92.unityshader";

	public enum ShaderName {
		PBS_Triplanar,
		PBS_TriplanarSpecular,
		PBS_TriplanarTransparent,
		PBS_TriplanarTransparentSpecular,
	}

	private static PBS_TriplanarMetallic PBS_TriplanarMetallic = new PBS_TriplanarMetallic();
	private static PBS_TriplanarSpecular PBS_TriplanarSpecular = new PBS_TriplanarSpecular();

	private static PBS_TriplanarMetallic metallic1 = new PBS_TriplanarMetallic();
	private static PBS_TriplanarMetallic metallic2 = new PBS_TriplanarMetallic();
	private static PBS_TriplanarSpecular specular1 = new PBS_TriplanarSpecular();
	private static PBS_TriplanarSpecular specular2 = new PBS_TriplanarSpecular();

	private static FrooxEngine.Shader? PBS_Triplanar_shader;
	private static FrooxEngine.Shader? PBS_TriplanarSpecular_shader;
	private static FrooxEngine.Shader? PBS_TriplanarTransparent_shader;
	private static FrooxEngine.Shader? PBS_TriplanarTransparentSpecular_shader;

	private static StaticShader? PBS_Triplanar_staticShader;
	private static StaticShader? PBS_TriplanarSpecular_staticShader;
	private static StaticShader? PBS_TriplanarTransparent_staticShader;
	private static StaticShader? PBS_TriplanarTransparentSpecular_staticShader;

	public static StaticShader? GetStaticShaderFromName(ShaderName name) {
		switch (name) {
			case ShaderName.PBS_Triplanar:
				return PBS_Triplanar_staticShader;
			case ShaderName.PBS_TriplanarSpecular:
				return PBS_TriplanarSpecular_staticShader;
			case ShaderName.PBS_TriplanarTransparent:
				return PBS_TriplanarTransparent_staticShader;
			case ShaderName.PBS_TriplanarTransparentSpecular:
				return PBS_TriplanarTransparentSpecular_staticShader;
		}
		return null;
	}

	private static FrooxEngine.Shader? GetShaderFromName(ShaderName name) {
		switch (name) {
			case ShaderName.PBS_Triplanar:
				return PBS_Triplanar_shader;
			case ShaderName.PBS_TriplanarSpecular:
				return PBS_TriplanarSpecular_shader;
			case ShaderName.PBS_TriplanarTransparent:
				return PBS_TriplanarTransparent_shader;
			case ShaderName.PBS_TriplanarTransparentSpecular:
				return PBS_TriplanarTransparentSpecular_shader;
		}
		return null;
	}

	private static bool SetStaticShaderFromName(ShaderName name, StaticShader staticShader) {
		switch (name) {
			case ShaderName.PBS_Triplanar:
				PBS_Triplanar_staticShader = staticShader;
				return true;
			case ShaderName.PBS_TriplanarSpecular:
				PBS_TriplanarSpecular_staticShader = staticShader;
				return true;
			case ShaderName.PBS_TriplanarTransparent:
				PBS_TriplanarTransparent_staticShader = staticShader;
				return true;
			case ShaderName.PBS_TriplanarTransparentSpecular:
				PBS_TriplanarTransparentSpecular_staticShader = staticShader;
				return true;
		}
		return false;
	}

	private static bool SetShaderFromName(ShaderName name, FrooxEngine.Shader shader) {
		switch (name) {
			case ShaderName.PBS_Triplanar:
				PBS_Triplanar_shader = shader;
				return true;
			case ShaderName.PBS_TriplanarSpecular:
				PBS_TriplanarSpecular_shader = shader;
				return true;
			case ShaderName.PBS_TriplanarTransparent:
				PBS_TriplanarTransparent_shader = shader;
				return true;
			case ShaderName.PBS_TriplanarTransparentSpecular:
				PBS_TriplanarTransparentSpecular_shader = shader;
				return true;
		}
		return false;
	}

	private static Uri GetUriFromName(ShaderName name) {
		switch (name) {
			case ShaderName.PBS_Triplanar:
				return new Uri(resdb_froox);
			case ShaderName.PBS_TriplanarSpecular:
				return new Uri(resdb_froox_specular);
			case ShaderName.PBS_TriplanarTransparent:
				return new Uri(resdb_froox_transparent);
			case ShaderName.PBS_TriplanarTransparentSpecular:
				return new Uri(resdb_froox_transparent_specular);
		}
		return null;
	}
	private static StaticShader createShaderComponent(string hash, Uri url, World world) {
		StaticShader sharedComponentOrCreate = world.GetSharedComponentOrCreate(hash, delegate (StaticShader provider)
		{
			provider.URL.Value = url;
		}, 0, true, false);
		sharedComponentOrCreate.Persistent = false;
		return sharedComponentOrCreate;
	}

	public static bool InitializeWithWorld(World world) {
		PBS_Triplanar_staticShader = createShaderComponent(choco, new Uri(resdb_choco), world);
		PBS_TriplanarSpecular_staticShader = createShaderComponent(choco_specular, new Uri(resdb_choco_specular), world);
		PBS_TriplanarTransparent_staticShader = createShaderComponent(choco_transparent, new Uri(resdb_choco_transparent), world);
		PBS_TriplanarTransparentSpecular_staticShader = createShaderComponent(choco_transparent_specular, new Uri(resdb_choco_transparent_specular), world);
		return true;
	}

	public static ShaderName? stringToName(string str) {
		if (str.Contains(froox))
			return ShaderName.PBS_Triplanar;
		if (str.Contains(froox_specular))
			return ShaderName.PBS_TriplanarSpecular;
		if (str.Contains(froox_transparent))
			return ShaderName.PBS_TriplanarTransparent;
		if (str.Contains(froox_transparent_specular))
			return ShaderName.PBS_TriplanarTransparentSpecular;
		return null;
	}
}
