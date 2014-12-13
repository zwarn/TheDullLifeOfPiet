/*
 * File:			GAFAssetPostProcessor.cs
 * Version:			3.9
 * Last changed:	2014/10/3 14:49
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEditor;
using UnityEngine;

using System.IO;
using System.Linq;

using GAF.Assets;
using GAF.Reader;

using GAFEditor.Tracking;
using GAFEditor.Utils;

namespace GAFEditor.Assets
{
	public class GAFAssetPostProcessor : AssetPostprocessor
	{
		public void OnPreprocessTexture()
		{
			GAFResourceManager.instance.preProcessTexture((TextureImporter)assetImporter);
		}

		public void OnPostprocessTexture(Texture2D _Texture)
		{
			GAFResourceManager.instance.postProcessTexture(assetPath, (TextureImporter)assetImporter);
		}

		public override uint GetVersion()
		{
			return (uint)1;
		}

		public static void OnPostprocessAllAssets(
			  string[] importedAssets
			, string[] deletedAssets
			, string[] movedAssets
			, string[] movedFromAssetPaths)
		{
			foreach (string assetName in importedAssets)
			{
				if (assetName.EndsWith(".gaf"))
				{
					byte[] fileBytes = null;
					using (BinaryReader freader = new BinaryReader(File.OpenRead(assetName)))
					{
						fileBytes = freader.ReadBytes((int)freader.BaseStream.Length);
					}

					if (fileBytes.Length > sizeof(int))
					{
						int header = System.BitConverter.ToInt32(fileBytes.Take(4).ToArray(), 0);
						if (GAFHeader.isCorrectHeader((GAFHeader.CompressionType)header))
						{
							var path = Path.GetDirectoryName(assetName) + "/" + Path.GetFileNameWithoutExtension(assetName) + ".asset";

							var animationAsset = ScriptableObject.CreateInstance<GAFAnimationAsset>();
							animationAsset = GAFAssetUtils.saveAsset(animationAsset, path);
							animationAsset.name = Path.GetFileNameWithoutExtension(assetName);
							animationAsset.initialize(fileBytes, AssetDatabase.AssetPathToGUID(path));

							GAFResourceManager.instance.createResources(animationAsset);

							GAFTracking.sendAssetCreatedRequest(assetName);
						}
					}
				}
			}
		}
	}
}