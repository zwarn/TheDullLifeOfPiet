/*
 * File:			gafresourcemanager.cs
 * Version:			3.10
 * Last changed:	2014/10/20 16:08
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using GAF;
using GAF.Assets;

using GAFEditor.Utils;

namespace GAFEditor.Assets
{
	[InitializeOnLoad]
	public class GAFResourceManager
	{
		#region Members

		private static GAFResourceManager m_Instance = null;

		private List<string> m_ImportList = new List<string>();
		private List<GAFTexturesResource> m_Resources = new List<GAFTexturesResource>();
		private GAFTaskManager m_TaskManager = new GAFTaskManager();

		#endregion // Members

		#region Interface

		public static GAFResourceManager instance
		{
			get
			{
				return m_Instance;
			}
		}

		public void createResources(GAFAnimationAsset _Asset)
		{
			var assetPath = AssetDatabase.GetAssetPath(_Asset);
			if (!string.IsNullOrEmpty(assetPath))
			{
				GAFSystem.getCachePath();

				var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
				var resourceTexturesNames = new Dictionary<KeyValuePair<float, float>, List<string>>();

				_Asset.resetGUID(assetGUID);

				foreach (var timeline in _Asset.getTimelines())
				{
					foreach (var atlas in timeline.atlases)
					{
						foreach (var data in atlas.texturesData.Values)
						{
							foreach (var textureInfo in data.files)
							{
								string textureName = Path.GetFileNameWithoutExtension(textureInfo.Value);
								var key = new KeyValuePair<float, float>(atlas.scale, textureInfo.Key);

								if (!resourceTexturesNames.ContainsKey(key))
									resourceTexturesNames[key] = new List<string>();

								resourceTexturesNames[key].Add(textureName);
							}
						}
					}
				}

				m_Resources.RemoveAll(resource => resource == null || !resource.isValid);

				foreach (var pair in resourceTexturesNames)
				{
					var name = _Asset.getResourceName(pair.Key.Key, pair.Key.Value) + ".asset";
					var path = GAFSystem.getCachePath() + name;
					var initialResDir = Path.GetDirectoryName(assetPath).Replace('\\', '/') + "/";

					var resource = ScriptableObject.CreateInstance<GAFTexturesResource>();
					resource = GAFAssetUtils.saveAsset(resource, path);
					resource.initialize(_Asset, pair.Value, pair.Key.Key, pair.Key.Value, initialResDir);
					EditorUtility.SetDirty(resource);

					findResourceTextures(resource);

					if (!resource.isReady)
						m_Resources.Add(resource);
				}

				EditorUtility.SetDirty(_Asset);
			}
		}

		public void deleteResources(GAFAnimationAsset _Asset)
		{
			var assetPath = AssetDatabase.GetAssetPath(_Asset);
			if (!string.IsNullOrEmpty(assetPath))
			{
				var resourcePaths = _Asset.resourcesPaths;
				foreach (var path in resourcePaths)
				{
					AssetDatabase.DeleteAsset(path);
				}

				_Asset.resetGUID(AssetDatabase.AssetPathToGUID(assetPath));
				EditorUtility.SetDirty(_Asset);
			}
		}

		public void findResourceTextures(GAFTexturesResource _Resource)
		{
			var resourcePath = AssetDatabase.GetAssetPath(_Resource);

			if (!string.IsNullOrEmpty(resourcePath))
			{
				var textures = GAFAssetUtils.findAssetsAtPath<Texture2D>(_Resource.currentDataPath, "*.png");
				foreach (var texture in textures)
				{
					var data = _Resource.missingData.Find(_data => _data.name == texture.name);
					if (data != null)
					{
						var texturePath = AssetDatabase.GetAssetPath(texture);
						var textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
						if (hasCorrectImportSettings(textureImporter, _Resource))
						{
							data.set(texture, getSharedMaterial(texture));
							EditorUtility.SetDirty(_Resource);
						}
						else
						{
							changeTextureImportSettings(textureImporter, _Resource);
							AssetDatabase.ImportAsset(textureImporter.assetPath, ImportAssetOptions.ForceUpdate);
						}
					}
				}
			}
		}

		public void reimportResourceTextures(GAFTexturesResource _Resource)
		{
			var resourcePath = AssetDatabase.GetAssetPath(_Resource);

			if (!string.IsNullOrEmpty(resourcePath))
			{
				var textures = GAFAssetUtils.findAssetsAtPath<Texture2D>(_Resource.currentDataPath, "*.png");
				foreach (var texture in textures)
				{
					var data = _Resource.data.Find(_data => _data.name == texture.name);
					if (data != null)
					{
						var texturePath = AssetDatabase.GetAssetPath(texture);
						var textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
						if (hasCorrectImportSettings(textureImporter, _Resource))
						{
							data.set(texture, getSharedMaterial(texture));
							EditorUtility.SetDirty(_Resource);
						}
						else
						{
							data.set(null, null);
							EditorUtility.SetDirty(_Resource);
							changeTextureImportSettings(textureImporter, _Resource);
							AssetDatabase.ImportAsset(textureImporter.assetPath, ImportAssetOptions.ForceUpdate);
						}
					}
				}
			}
		}

		public Material getSharedMaterial(Texture2D _Texture)
		{
			string texturePath = AssetDatabase.GetAssetPath(_Texture);
			string path = Path.GetDirectoryName(texturePath) + "/" + Path.GetFileNameWithoutExtension(texturePath) + ".mat";

			var material = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
			if (material == null)
			{
				material = new Material(Shader.Find("GAF/GAFObjectsGroup"));
				material.mainTexture = _Texture;
				material = GAFAssetUtils.saveAsset(material, path);
			}
			else
			{
				material.mainTexture = _Texture;
			}

			return material;
		}

		public void preProcessTexture(TextureImporter _Importer)
		{
			var textureName = Path.GetFileNameWithoutExtension(_Importer.assetPath);

			m_Resources.RemoveAll(resource => resource == null || !resource.isValid);

			foreach (var resource in m_Resources)
			{
				if (resource.currentDataPath == Path.GetDirectoryName(_Importer.assetPath) + "/")
				{
					var data = resource.missingData.Find(_data => _data.name == textureName);
					if (data != null)
					{
						if (!hasCorrectImportSettings(_Importer, resource))
						{
							changeTextureImportSettings(_Importer, resource);
						}
					}
				}
			}
		}

		public void postProcessTexture(string _TexturePath, TextureImporter _Importer)
		{
			m_TaskManager.waitFor(0f).then(() => postProcessTextureDelayed(_TexturePath, _Importer));
		}

		#endregion // Inteface

		#region Implementation

		static GAFResourceManager()
		{
			m_Instance = new GAFResourceManager();
		}

		private GAFResourceManager()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				m_TaskManager.waitFor(0f).then(() => resetResources());
			}
		}

		private void resetResources()
		{
			m_Resources.Clear();

			m_Resources = AssetDatabase.FindAssets("t:GAFTexturesResource").ToList().ConvertAll(
				resourceGUID => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(resourceGUID), typeof(GAFTexturesResource)) as GAFTexturesResource);

			var invalid = m_Resources.FindAll(resource => !resource.isValid);
			m_Resources.RemoveAll(resource => !resource.isValid);

			foreach (var invalidResource in invalid)
			{
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(invalidResource));
			}
		}

		private void postProcessTextureDelayed(string _TexturePath, TextureImporter _Importer)
		{
			var texture = AssetDatabase.LoadAssetAtPath(_TexturePath, typeof(Texture2D)) as Texture2D;

			m_Resources.RemoveAll(resource => resource == null || !resource.isValid);

			foreach (var resource in m_Resources)
			{
				if (resource.currentDataPath == Path.GetDirectoryName(_TexturePath) + "/")
				{
					var data = resource.missingData.Find(_data => _data.name == texture.name);
					if (data != null)
					{
						if (hasCorrectImportSettings(_Importer, resource))
						{
							data.set(texture, getSharedMaterial(texture));
							EditorUtility.SetDirty(resource);
						}
						else
						{
							changeTextureImportSettings(_Importer, resource);
							AssetDatabase.ImportAsset(_Importer.assetPath, ImportAssetOptions.ForceUpdate);
						}
					}
				}
			}
		}

		private void changeTextureImportSettings(TextureImporter _Importer, GAFTexturesResource _Resource)
		{
			if (!m_ImportList.Contains(_Importer.assetPath))
			{
				_Importer.textureType			= TextureImporterType.Advanced;
				_Importer.npotScale				= TextureImporterNPOTScale.None;
				_Importer.mipmapEnabled			= false;
				_Importer.maxTextureSize		= 4096;
				_Importer.alphaIsTransparency	= true;
				_Importer.isReadable			= true;
				_Importer.textureFormat			= TextureImporterFormat.ARGB32;

				TextureImporterSettings st = new TextureImporterSettings();
				_Importer.ReadTextureSettings(st);
				st.wrapMode = TextureWrapMode.Clamp;
				_Importer.SetTextureSettings(st);
			}
		}

		private bool hasCorrectImportSettings(TextureImporter _Importer, GAFTexturesResource _Resource)
		{
			return  _Importer.textureType			== TextureImporterType.Advanced &&
					_Importer.npotScale				== TextureImporterNPOTScale.None &&
					_Importer.mipmapEnabled			== false &&
					_Importer.maxTextureSize		== 4096 &&
					_Importer.alphaIsTransparency	== true &&
					_Importer.isReadable			== true &&
					_Importer.textureFormat			== TextureImporterFormat.ARGB32;
		}

		#endregion // Implementation
	}
}