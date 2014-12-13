/*
 * File:			GAFAnimationAssetEditor.cs
 * Version:			3.9
 * Last changed:	2014/10/17 10:51
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEngine;
using UnityEditor;

using System.Linq;
using System.IO;
using System.Collections.Generic;

using GAF;
using GAF.Core;
using GAF.Assets;

using GAFEditor.Assets;
using GAFEditor.ExternalEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(GAFAnimationAsset))]
public class GAFAnimationAssetEditor : Editor
{
	new public GAFAnimationAsset target
	{
		get
		{
			return base.target as GAFAnimationAsset;
		}
	}

	new public List<GAFAnimationAsset> targets
	{
		get
		{
			return base.targets.ToList().ConvertAll(_target => _target as GAFAnimationAsset);
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var nameProperty = serializedObject.FindProperty("m_Name");

		GUILayout.Space(5f);
		EditorGUILayout.LabelField("Asset name: " + (!nameProperty.hasMultipleDifferentValues ? nameProperty.stringValue : "—"), EditorStyles.boldLabel);

		var loadedCount = targets.Where(_target => _target.isLoaded).Count();
		if (loadedCount == targets.Count)
		{
			if (loadedCount == 1)
			{
				drawAssetData();

				GUILayout.Space(5f);
				drawTimelines();

				GUILayout.Space(5f);
				drawButtons();

				GUILayout.Space(5f);
				drawResources();
			}
			else
			{
				GUILayout.Space(5f);
				EditorGUILayout.HelpBox("Cannot view multiple asset settings.", MessageType.Info);

				GUILayout.Space(5f);
				drawButtons();
			}
		}
		else if (loadedCount == 0)
		{
			GUILayout.Space(5f);
			EditorGUILayout.HelpBox("Asset(s) is(are) not loaded! Please reload asset(s) or reimport '.gaf' file.", MessageType.Warning);
		}
		else
		{
			GUILayout.Space(5f);
			EditorGUILayout.HelpBox("Some of assets are not loaded! Please reload asset(s) or reimport '.gaf' file.", MessageType.Warning);
		}

		GUI.enabled = loadedCount == targets.Count;
		EditorGUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Delete resources"))
			{
				foreach(var _target in targets)
					GAFResourceManager.instance.deleteResources(_target);
			}

			if (GUILayout.Button("Rebuild resources"))
			{
				foreach (var _target in targets)
					GAFResourceManager.instance.createResources(_target);
			}
		}
		EditorGUILayout.EndHorizontal();
		GUI.enabled = true;
	}

	private void drawResources()
	{
		EditorGUILayout.LabelField("Resources: ");
		foreach (var resourcePath in target.resourcesPaths)
		{
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.LabelField(resourcePath);
				var resource = AssetDatabase.LoadAssetAtPath(resourcePath, typeof(GAFTexturesResource)) as GAFTexturesResource;
				if (resource != null)
				{
					var textures	= resource.data.Select(data => data.sharedTexture).ToList();
					var materials	= resource.data.Select(data => data.sharedMaterial).ToList();
					var invalidData = resource.data.Where(data => !data.isValid).Select(data => data.name).ToList();

					if (textures.Count > 0)
					{
						GUILayout.Space(3f);
						EditorGUILayout.LabelField("Found textures: ");
						for (int index = 0; index < textures.Count; ++index)
						{
							string path = AssetDatabase.GetAssetPath(textures[index]);

							EditorGUILayout.LabelField("\t" + (index + 1).ToString() + ". " + path);
						}
					}

					if (invalidData.Count > 0)
					{
						GUILayout.Space(3f);
						EditorGUILayout.LabelField("Missing textures: ");
						for (int index = 0; index < invalidData.Count; ++index)
						{
							EditorGUILayout.LabelField("\t" + (index + 1).ToString() + ". " + invalidData[index]);
						}
					}

					if (materials.Count > 0)
					{
						GUILayout.Space(3f);
						EditorGUILayout.LabelField("Shared materials: ");
						for (int index = 0; index < materials.Count; ++index)
						{
							string path = AssetDatabase.GetAssetPath(materials[index]);

							EditorGUILayout.LabelField("\t" + (index + 1).ToString() + ". " + path);
						}
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
	}

	private void drawTimelines()
	{
		EditorGUILayout.LabelField("Timelines: ");
		foreach (var timeline in target.getTimelines())
		{
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				GUILayout.Space(5f);
				EditorGUILayout.LabelField("ID - " + timeline.id.ToString());
				EditorGUILayout.LabelField("Linkage name - " + timeline.linkageName);

				GUILayout.Space(5f);
				EditorGUILayout.LabelField("Frame size: " + timeline.frameSize.ToString());
				EditorGUILayout.LabelField("Pivot: " + timeline.pivot.ToString());
				EditorGUILayout.LabelField("Frames count: " + timeline.framesCount.ToString());

				GUILayout.Space(5f);
				EditorGUILayout.LabelField("Available sequences: " + string.Join(",", timeline.sequences.ConvertAll(sequence => sequence.name).ToArray()));

				GUILayout.Space(5f);
				EditorGUILayout.LabelField("Objects count: " + timeline.objects.Count.ToString());
				EditorGUILayout.LabelField("Masks count: " + timeline.masks.Count.ToString());
			}
			EditorGUILayout.EndVertical();
		}
	}

	private void drawButtons()
	{
		EditorGUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Add to scene"))
			{
				foreach (var _target in targets)
					addToScene(_target);
			}

			GUILayout.Space(5f);
			if (GUILayout.Button("Create prefab"))
			{
				foreach (var _target in targets)
					createPrefab(_target);
			}

			GUILayout.Space(5f);
			if (GUILayout.Button("Prefab+instance"))
			{
				foreach (var _target in targets)
					createPrefabPlusInstance(_target);
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	private void drawAssetData()
	{
		EditorGUILayout.BeginVertical(EditorStyles.textField);
		{
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("GAF version: " + target.majorDataVersion.ToString() + "." + target.minorDataVersion.ToString());
			EditorGUILayout.LabelField("Asset version: " + target.assetVersion.ToString());

			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Available atlas scales: " + string.Join(",", target.scales.ConvertAll(scale => scale.ToString()).ToArray()));
			EditorGUILayout.LabelField("Available content scale factors: " + string.Join(",", target.csfs.ConvertAll(csf => csf.ToString()).ToArray()));

			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Batchable objects count: " + target.batchableObjects.Count.ToString());
			EditorGUILayout.LabelField("Masked objects count: " + target.maskedObjects.Count.ToString());
		}
		EditorGUILayout.EndVertical();
	}

	private void addToScene(GAFAnimationAsset _Asset)
	{
		createMovieClip(_Asset);
	}

	private void createPrefab(GAFAnimationAsset _Asset)
	{
		var path = AssetDatabase.GetAssetPath(target);
		path = path.Substring(0, path.Length - name.Length - ".asset".Length);

		var prefabPath = path + name + ".prefab";
		var existingPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		if (existingPrefab == null)
		{
			var movieClipObject = createMovieClip(_Asset);
			var prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
			prefab = PrefabUtility.ReplacePrefab(movieClipObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
			GameObject.DestroyImmediate(movieClipObject);
		}
	}

	private void createPrefabPlusInstance(GAFAnimationAsset _Asset)
	{
		var path = AssetDatabase.GetAssetPath(target);
		path = path.Substring(0, path.Length - name.Length - ".asset".Length);

		var prefabPath = path + name + ".prefab";
		var existingPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		if (existingPrefab == null)
		{
			var movieClipObject = createMovieClip(_Asset);
			var prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
			prefab = PrefabUtility.ReplacePrefab(movieClipObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
		}
		else
		{
			PrefabUtility.InstantiatePrefab(existingPrefab);
		}
	}

	private GameObject createMovieClip(GAFAnimationAsset _Asset)
	{
		var movieClipObject = new GameObject(_Asset.name);
		var movieClip = movieClipObject.AddComponent<GAFMovieClip>();

		movieClip.initialize(_Asset, 0);
		movieClip.reload();

		return movieClipObject;
	}
}
