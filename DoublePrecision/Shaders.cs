using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FrooxEngine;

namespace DoublePrecision;
static class Shaders {
	private const string prefix = "resdb:///";
	private const string choco_suffix = ".__Choco__Shader!";
	private const string froox_suffix = ".unityshader";

	public const string choco = "2f72119e76f9f16bdc119298c1bd64c4fb089921cefc6a6a91150fa9dadd6061";
	public const string choco_specular = "cc46975c2de65f71dafe78024f4a3168a30ec47635f7dad0a9842c156c0295c2";
	public const string choco_transparent = "3b553e09ee36081a44668cf5063930c2bb2bf94e4bb114cdd550b50eb97380ce";
	public const string choco_transparent_specular = "f6cc643ed5589c44b686434c70fe3c51cef6ad131d4bddf6d34e476849e16432";

	public const string froox = "d9d43057b97ff2e71b9947af38c754cde992bfa8ea75ab34d9e43859e0a0f7d3";
	public const string froox_specular = "33dcd39d588e92840eb58845d3a0404e75c038e277a92b634721dcecc16dfd9a";
	public const string froox_transparent = "abfef7119e75779d7ad31222211acc4dbcced41bacda4fd32bf04fac633c8b1b";
	public const string froox_transparent_specular = "205b3cc9c239927986895a41e6cc7323853da09b87d23c5b466a2940b4e4de92";

	public const string ext_choco = choco + choco_suffix;
	public const string ext_choco_specular = choco_specular + choco_suffix;
	public const string ext_choco_transparent = choco_transparent + choco_suffix;
	public const string ext_choco_transparent_specular = choco_transparent_specular + choco_suffix;

	public const string ext_froox = froox + froox_suffix;
	public const string ext_froox_specular = froox_specular + froox_suffix;
	public const string ext_froox_transparent = froox_transparent + froox_suffix;
	public const string ext_froox_transparent_specular = froox_transparent_specular + froox_suffix;

	public const string resdb_choco = prefix + ext_choco;
	public const string resdb_choco_specular = prefix + ext_choco_specular;
	public const string resdb_choco_transparent = prefix + ext_choco_transparent;
	public const string resdb_choco_transparent_specular = prefix + ext_choco_transparent_specular;

	public const string resdb_froox = prefix + ext_froox;
	public const string resdb_froox_specular = prefix + ext_froox_specular;
	public const string resdb_froox_transparent = prefix + ext_froox_transparent;
	public const string resdb_froox_transparent_specular = prefix + ext_froox_transparent_specular;
}
