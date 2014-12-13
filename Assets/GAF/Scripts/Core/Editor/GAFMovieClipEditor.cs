/*
 * File:			gafmovieclipeditor.cs
 * Version:			3.10
 * Last changed:	2014/10/20 17:31
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using GAF;
using GAF.Core;
using GAF.Assets;

using GAFEditor.Tracking;
using GAFEditor.Assets;

namespace GAFEditor.Core
{
	[CustomEditor(typeof(GAFMovieClip))]
	[CanEditMultipleObjects]
	public class GAFMovieClipEditor : Editor
	{
		#region Members

		private int m_TimelineIndex = 0;
		private int m_TimelineID = 0;

		private bool m_ShowSettings				= true;
		private bool m_ShowSequences			= true;
		private bool m_ShowPlayback				= true;

		#endregion // Members

		#region Properties

		new public List<GAFMovieClip> targets
		{
			get
			{
				return base.targets.ToList().ConvertAll(target => (GAFMovieClip)target);
			}
		}

		#endregion // Properties

		#region Interface

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			drawAsset();
			drawResourcesState();
			drawSettings();
			drawSequences();
			drawPlayback();
			drawDataButtons();

			//drawRegisterButton();

			serializedObject.ApplyModifiedProperties();
		}

		#endregion // Interface

		#region Implementation

		private void drawAsset()
		{
			var initializedProperty = serializedObject.FindProperty("m_IsInitialized");
			var assetProperty = serializedObject.FindProperty("m_GAFAsset");

			if (!initializedProperty.hasMultipleDifferentValues)
			{
				if (!initializedProperty.boolValue)
				{
					if (hasPrefabs())
					{
						GUILayout.Space(10f);
						EditorGUILayout.BeginVertical(EditorStyles.textField);
						{
							EditorGUILayout.HelpBox("Cannot init movie clip in prefab!", MessageType.Warning);
						}
						EditorGUILayout.EndVertical();
					}
					else
					{
						GUILayout.Space(10f);
						EditorGUILayout.PropertyField(assetProperty, new GUIContent("Asset:"));

						if (assetProperty.objectReferenceValue != null &&
							!assetProperty.hasMultipleDifferentValues)
						{
							var asset = (GAFAnimationAsset)assetProperty.objectReferenceValue;
							if (!asset.isLoaded)
							{
								drawAssetIsNotLoaded(asset);
							}
							else
							{
								drawChooseTimeline(asset);
								drawInitMovieClipButton(asset);
							}
						}
					}
				}
				else
				{
					if (!assetProperty.hasMultipleDifferentValues)
					{
						var asset = (GAFAnimationAsset)assetProperty.objectReferenceValue;
						if (asset != null)
						{
							if (!asset.isLoaded)
							{
								drawAssetIsNotLoaded(asset);
							}
							else
							{
								GUILayout.Space(10f);
								var newAsset = EditorGUILayout.ObjectField("Asset: ", asset, typeof(GAFAnimationAsset), false) as GAFAnimationAsset;
								if (newAsset != asset)
								{
									foreach (var _target in targets)
										_target.clear(true);

									if (newAsset != null && newAsset.isLoaded)
									{
										foreach (var _target in targets)
										{
											_target.initialize(newAsset, _target.timelineID);
											_target.reload();
										}
									}
								}
							}
						}
						else
						{
							GUILayout.Space(10f);
							EditorGUILayout.BeginVertical(EditorStyles.textField);
							{
								EditorGUILayout.LabelField("Asset is not found!", EditorStyles.boldLabel);
							}
							EditorGUILayout.EndVertical();
						}
					}
					else
					{
						GUILayout.Space(10f);
						EditorGUILayout.BeginVertical(EditorStyles.textField);
						{
							EditorGUILayout.HelpBox("Multiple assets...", MessageType.Info);
						}
						EditorGUILayout.EndVertical();
					}
				}
			}
			else
			{
				GUILayout.Space(10f);
				EditorGUILayout.BeginVertical(EditorStyles.textField);
				{
					EditorGUILayout.HelpBox("Different clip states...", MessageType.Info);
				}
				EditorGUILayout.EndVertical();
			}
		}

		private void drawResourcesState()
		{
			int initializedCount = targets.Where(clip => clip.isInitialized).Count();
			if (initializedCount == targets.Count)
			{
				int validResourcesCount = targets.Where(clip => clip.resource != null && clip.resource.isValid && clip.resource.isReady).Count();
				if (validResourcesCount == 0)
				{
					drawResourcesMissing();
				}
				else if (validResourcesCount != targets.Count)
				{
					drawDifferentResourcesState();
				}
				else
				{
					drawCorrectResourcesState();
				}
			}
		}

		private void drawSettings()
		{
			GUILayout.Space(3f);
			m_ShowSettings = EditorGUILayout.Foldout(m_ShowSettings, "Settings");

			if (m_ShowSettings)
			{
				var settingProperty = serializedObject.FindProperty("m_Settings");
				EditorGUILayout.BeginVertical(EditorStyles.textField);
				{
					GUILayout.Space(3f);
					drawProperty(settingProperty.FindPropertyRelative("m_PlayAutomatically"), "Play automatically: ");

					GUILayout.Space(3f);
					drawProperty(settingProperty.FindPropertyRelative("m_IgnoreTimeScale"), "Ignore time scale: ");

					GUILayout.Space(3f);
					drawProperty(settingProperty.FindPropertyRelative("m_PerfectTiming"), "Perfect timing (possible frames skip): ");

					GUILayout.Space(3f);
					drawProperty(settingProperty.FindPropertyRelative("m_PlayInBackground"), "Play in backgound: ");

					GUILayout.Space(10f);
					drawProperty(settingProperty.FindPropertyRelative("m_WrapMode"), "Wrap mode: ");

					GUILayout.Space(3f);
					EditorGUI.BeginChangeCheck();
					var fpsProperty = settingProperty.FindPropertyRelative("m_TargetFPS");
					drawProperty(fpsProperty, "Target FPS: ");
					if (EditorGUI.EndChangeCheck())
					{
						fpsProperty.intValue = Mathf.Clamp(fpsProperty.intValue, 0, 1000);
						serializedObject.ApplyModifiedProperties();
					}

					GUILayout.Space(10f);
					drawSortingID();

					GUILayout.Space(3f);
					EditorGUI.BeginChangeCheck();
					var layerOrderProperty = settingProperty.FindPropertyRelative("m_SpriteLayerValue");
					drawProperty(layerOrderProperty, "Sorting layer order: ");
					if (EditorGUI.EndChangeCheck())
					{
						serializedObject.ApplyModifiedProperties();
						reloadTargets();
					}

					GUILayout.Space(10f);
					EditorGUI.BeginChangeCheck();
					var pixelsPerUnitProperty = settingProperty.FindPropertyRelative("m_PixelsPerUnit");
					drawProperty(pixelsPerUnitProperty, "Pixels per unit: ");
					if (EditorGUI.EndChangeCheck())
					{
						pixelsPerUnitProperty.floatValue = Mathf.Clamp(pixelsPerUnitProperty.floatValue, 0.001f, 1000f);
						serializedObject.ApplyModifiedProperties();
						reloadTargets();
					}

					var assetProperty = serializedObject.FindProperty("m_GAFAsset");
					if (!assetProperty.hasMultipleDifferentValues &&
						 assetProperty.objectReferenceValue != null)
					{
						drawScales(assetProperty);
						drawCsfs(assetProperty);
					}
				}
				EditorGUILayout.EndVertical();
			}
		}

		private void drawSequences()
		{
			var initializedProperty = serializedObject.FindProperty("m_IsInitialized");
			var assetProperty = serializedObject.FindProperty("m_GAFAsset");
			var timelineProperty = serializedObject.FindProperty("m_TimelineID");
			var currentSequenceIndex = serializedObject.FindProperty("m_SequenceIndex");

			if (!initializedProperty.hasMultipleDifferentValues &&
				 initializedProperty.boolValue &&
				!assetProperty.hasMultipleDifferentValues &&
				 assetProperty.objectReferenceValue != null &&
				!timelineProperty.hasMultipleDifferentValues)
			{
				var asset = (GAFAnimationAsset)assetProperty.objectReferenceValue;
				var timelineID = timelineProperty.intValue;
				var sequences = asset.getSequences(timelineID);
				var sequenceNames = sequences.ConvertAll(_sequence => _sequence.name);
				var currentIndex = currentSequenceIndex.intValue;

				if (currentSequenceIndex.hasMultipleDifferentValues)
				{
					sequenceNames.Insert(0, "—");
					currentIndex = 0;
				}

				GUILayout.Space(3f);
				m_ShowSequences = EditorGUILayout.Foldout(m_ShowSequences, "Sequences");
				if (m_ShowSequences)
				{
					EditorGUILayout.BeginVertical(EditorStyles.textField);
					{
						GUILayout.Space(3f);
						EditorGUILayout.BeginHorizontal();
						{
							var style = currentSequenceIndex.isInstantiatedPrefab && currentSequenceIndex.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;

							EditorGUILayout.LabelField("Sequence:", style);
							var index = EditorGUILayout.Popup(currentIndex, sequenceNames.ToArray());
							if (index != currentIndex)
							{
								index = currentSequenceIndex.hasMultipleDifferentValues ? index - 1 : index;
								foreach (var target in targets)
								{
									target.setSequence(sequences[index].name);
									EditorUtility.SetDirty(target);
								}

								currentSequenceIndex.intValue = index;

								var currentFrameProperty = serializedObject.FindProperty("m_CurrentFrameNumber");
								currentFrameProperty.intValue = (int)sequences[index].startFrame;

								serializedObject.ApplyModifiedProperties();
							}
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
			}
		}

		private void drawPlayback()
		{
			var initializedProperty = serializedObject.FindProperty("m_IsInitialized");
			var assetProperty = serializedObject.FindProperty("m_GAFAsset");
			var timelineProperty = serializedObject.FindProperty("m_TimelineID");
			var currentSequenceIndex = serializedObject.FindProperty("m_SequenceIndex");

			if (!initializedProperty.hasMultipleDifferentValues &&
				initializedProperty.boolValue &&
				!assetProperty.hasMultipleDifferentValues &&
				 assetProperty.objectReferenceValue != null &&
				!timelineProperty.hasMultipleDifferentValues &&
				!currentSequenceIndex.hasMultipleDifferentValues)
			{
				var asset = (GAFAnimationAsset)assetProperty.objectReferenceValue;
				var timelineID = timelineProperty.intValue;
				var sequences = asset.getSequences(timelineID);
				var currentSequence = sequences[currentSequenceIndex.intValue];

				var currentFrameProperty = serializedObject.FindProperty("m_CurrentFrameNumber");

				GUILayout.Space(3f);
				m_ShowPlayback = EditorGUILayout.Foldout(m_ShowPlayback, "Playback");
				if (m_ShowPlayback)
				{
					EditorGUILayout.BeginVertical(EditorStyles.textField);
					{
						GUILayout.Space(3f);
						EditorGUILayout.LabelField("Frames timeline:");

						if (!Application.isPlaying)
						{
							EditorGUI.BeginChangeCheck();
							EditorGUI.showMixedValue = currentFrameProperty.hasMultipleDifferentValues;
							EditorGUILayout.IntSlider(currentFrameProperty, (int)currentSequence.startFrame, (int)currentSequence.endFrame, new GUIContent(""));
							EditorGUI.showMixedValue = false;
							if (EditorGUI.EndChangeCheck())
							{
								foreach (var target in targets)
								{
									target.gotoAndStop((uint)currentFrameProperty.intValue);
								}
								serializedObject.ApplyModifiedProperties();
							}
						}
						else
						{
							if (!currentFrameProperty.hasMultipleDifferentValues)
								EditorGUILayout.IntSlider(currentFrameProperty.intValue, (int)currentSequence.startFrame, (int)currentSequence.endFrame);
						}

						if (!currentFrameProperty.hasMultipleDifferentValues)
						{
							EditorGUILayout.BeginHorizontal();
							{
								var currentEnabled = GUI.enabled;
								GUI.enabled = currentEnabled && (uint)currentFrameProperty.intValue > currentSequence.startFrame;
								if (GUILayout.Button("<<", EditorStyles.miniButtonLeft))
								{
									foreach (var target in targets)
									{
										target.gotoAndStop(currentSequence.startFrame);
										EditorUtility.SetDirty(target);
									}
								}
								GUI.enabled = currentEnabled;

								GUI.enabled = currentEnabled && (uint)currentFrameProperty.intValue > currentSequence.startFrame;
								if (GUILayout.Button("<", EditorStyles.miniButtonMid))
								{
									foreach (var target in targets)
									{
										target.gotoAndStop((uint)(currentFrameProperty.intValue - 1));
										EditorUtility.SetDirty(target);
									}
								}
								GUI.enabled = currentEnabled;

								GUI.enabled = currentEnabled && (uint)currentFrameProperty.intValue < currentSequence.endFrame;
								if (GUILayout.Button(">", EditorStyles.miniButtonMid))
								{
									foreach (var target in targets)
									{
										target.gotoAndStop((uint)(currentFrameProperty.intValue + 1));
										EditorUtility.SetDirty(target);
									}
								}
								GUI.enabled = currentEnabled;

								GUI.enabled = currentEnabled && (uint)currentFrameProperty.intValue < currentSequence.endFrame;
								if (GUILayout.Button(">>", EditorStyles.miniButtonRight))
								{
									foreach (var target in targets)
									{
										target.gotoAndStop(currentSequence.endFrame);
										EditorUtility.SetDirty(target);
									}
								}
								GUI.enabled = currentEnabled;
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					EditorGUILayout.EndVertical();
				}
			}
		}

		private void drawDataButtons()
		{
			var initializedProperty = serializedObject.FindProperty("m_IsInitialized");
			var assetProperty = serializedObject.FindProperty("m_GAFAsset");

			if (!initializedProperty.hasMultipleDifferentValues &&
				 initializedProperty.boolValue &&
				!assetProperty.hasMultipleDifferentValues &&
				 assetProperty.objectReferenceValue != null)
			{
				var asset = (GAFAnimationAsset)assetProperty.objectReferenceValue;
				GUILayout.Space(7f);
				drawBuildResources(asset);
				drawReloadAnimationButton();
				drawClearButtons();
			}
		}

		private void drawRegisterButton()
		{
			var style = new GUIStyle(GUI.skin.button);
			style.fontSize = 12;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			style.hover.textColor = Color.white;
			style.focused.textColor = Color.white;
			style.active.textColor = Color.white;

			GUILayout.Space(15f);
			GUI.backgroundColor = new Color(0.016f, 0.423f, 0.537f, 1);
			if (GUILayout.Button("Register and get 500 conversions FREE!", style, GUILayout.Height(30f)))
			{
				Application.OpenURL("http://gafmedia.com/register/promo=unity3d_500");
			}
			GUILayout.Space(7f);
		}

		private void drawAssetIsNotLoaded(GAFAnimationAsset _Asset)
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.LabelField("Asset '" + _Asset.name + "' is not loaded properly! Try to reimport .GAF file!");
			}
			EditorGUILayout.EndVertical();
		}

		private void drawResourcesMissing()
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.HelpBox("Your animation(s) missing resources! \nImport necessary textures OR Build resources!", MessageType.Warning);
			}
			EditorGUILayout.EndVertical();
		}

		private void drawDifferentResourcesState()
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.HelpBox("Some of selected movie clips misses resource!", MessageType.Warning);
			}
			EditorGUILayout.EndVertical();
		}

		private void drawCorrectResourcesState()
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.HelpBox("Resources are available!", MessageType.Info);
			}
			EditorGUILayout.EndVertical();
		}

		private void drawBuildResources(GAFAnimationAsset _Asset)
		{
			GUILayout.Space(3f);

			var enabled = GUI.enabled;
			if (GUILayout.Button("Build resources"))
			{
				GAFResourceManager.instance.createResources(_Asset);

				foreach (var _target in targets)
					_target.reload();
			}
			GUI.enabled = enabled;
		}		

		private void drawChooseTimeline(GAFAnimationAsset _Asset)
		{
			if (_Asset.getTimelines().Count > 1)
			{
				GUILayout.Space(6f);
				EditorGUILayout.BeginVertical(EditorStyles.textField);
				{
					EditorGUILayout.LabelField("Choose timeline ID:");
					EditorGUILayout.BeginHorizontal();
					{
						var timelineIDs = _Asset.getTimelines().ConvertAll(timeline => timeline.id.ToString() + (timeline.linkageName.Length > 0 ? " - " + timeline.linkageName : "")).ToArray();
						var index = GUILayout.SelectionGrid(m_TimelineIndex, timelineIDs, timelineIDs.Length < 4 ? timelineIDs.Length : 4);
						if (index != m_TimelineIndex)
						{
							m_TimelineIndex = index;
							var timeline = timelineIDs[index];
							m_TimelineID = timeline.IndexOf(" - ") > 0 ? int.Parse(timeline.Split('-')[0]) : int.Parse(timeline);
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			else
			{
				GUILayout.Space(6f);
				EditorGUILayout.BeginVertical(EditorStyles.textField);
				{
					EditorGUILayout.LabelField("Timeline ID: 0 - rootTimeline");
					m_TimelineID = 0;
					m_TimelineIndex = 0;
				}
				EditorGUILayout.EndVertical();
			}
		}

		private void drawInitMovieClipButton(GAFAnimationAsset _Asset)
		{
			GUILayout.Space(3f);
			if (GUILayout.Button("Create GAF movie clip"))
			{
				foreach (var target in targets)
				{
					target.initialize(_Asset, m_TimelineID);
					target.reload();

					GAFTracking.sendMovieClipCreatedRequest(_Asset.name);
				}
			}
		}

		private void drawSortingID()
		{
			var settingProperty = serializedObject.FindProperty("m_Settings");
			var sortinglayerIDProperty = settingProperty.FindPropertyRelative("m_SpriteLayerID");

			List<string> layerNames = getSortingLayerNames().ToList();
			List<int> layerID = getSortingLayerUniqueIDs().ToList();

			var index = -1;
			if (sortinglayerIDProperty.hasMultipleDifferentValues)
				layerNames.Insert(0, "—");
			else
				index = layerID.FindIndex(__id => __id == sortinglayerIDProperty.intValue);

			if (index < 0)
				index = layerID.FindIndex(__id => __id == 0);

			EditorGUILayout.BeginHorizontal();
			{
				var style = sortinglayerIDProperty.isInstantiatedPrefab && sortinglayerIDProperty.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
				EditorGUILayout.LabelField("Sorting layer: ", style);
				var nextIndex = EditorGUILayout.Popup(index, layerNames.ToArray());
				if (index != nextIndex)
				{
					sortinglayerIDProperty.intValue = layerID[sortinglayerIDProperty.hasMultipleDifferentValues ? nextIndex - 1 : nextIndex];
					serializedObject.ApplyModifiedProperties();
					reloadTargets();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void drawScales(SerializedProperty _AssetProperty)
		{
			var asset = (GAFAnimationAsset)_AssetProperty.objectReferenceValue;
			var settingProperty = serializedObject.FindProperty("m_Settings");

			var scaleProperty = settingProperty.FindPropertyRelative("m_Scale");
			var scales = asset.scales.ConvertAll(__scale => __scale.ToString());

			var index = 0;
			if (scaleProperty.hasMultipleDifferentValues)
				scales.Insert(0, "—");
			else
				index = asset.scales.FindIndex(__scale => __scale == scaleProperty.floatValue);

			GUILayout.Space(3f);
			EditorGUILayout.BeginHorizontal();
			{
				var style = scaleProperty.isInstantiatedPrefab && scaleProperty.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
				EditorGUILayout.LabelField("Texture atlas scale: ", style);
				var nextIndex = EditorGUILayout.Popup(index, scales.ToArray());
				if (index != nextIndex)
				{
					scaleProperty.floatValue = asset.scales[scaleProperty.hasMultipleDifferentValues ? nextIndex - 1 : nextIndex];
					serializedObject.ApplyModifiedProperties();
					reloadTargets();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void drawCsfs(SerializedProperty _AssetProperty)
		{
			var asset = (GAFAnimationAsset)_AssetProperty.objectReferenceValue;
			var settingProperty = serializedObject.FindProperty("m_Settings");

			var csfProperty = settingProperty.FindPropertyRelative("m_CSF");
			var csfs = asset.csfs.ConvertAll(__csf => __csf.ToString());

			var index = 0;
			if (csfProperty.hasMultipleDifferentValues)
				csfs.Insert(0, "—");
			else
				index = asset.csfs.FindIndex(__csf => __csf == csfProperty.floatValue);

			GUILayout.Space(3f);
			EditorGUILayout.BeginHorizontal();
			{
				var style = csfProperty.isInstantiatedPrefab && csfProperty.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
				EditorGUILayout.LabelField("Content scale factor: ", style);
				var nextIndex = EditorGUILayout.Popup(index, csfs.ToArray());
				if (index != nextIndex)
				{
					csfProperty.floatValue = asset.csfs[csfProperty.hasMultipleDifferentValues ? nextIndex - 1 : nextIndex];
					serializedObject.ApplyModifiedProperties();
					reloadTargets();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void drawReloadAnimationButton()
		{
			GUILayout.Space(3f);
			if (GUILayout.Button("Reload animation"))
			{
				reloadTargets();
			}
		}

		private void drawClearButtons()
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Clear animation"))
				{
					foreach (var target in targets)
					{
						if (target.isInitialized && PrefabUtility.GetPrefabType(target.gameObject) != PrefabType.Prefab)
						{
							target.clear();
							clearObjectManagerLists(target);
							EditorUtility.SetDirty(target);
						}
					}

					m_TimelineIndex = 0;
					m_TimelineID = 0;
				}

				if (GUILayout.Button("Clear animation (delete children)"))
				{
					foreach (var target in targets)
					{
						if (target.isInitialized && PrefabUtility.GetPrefabType(target.gameObject) != PrefabType.Prefab)
						{
							target.clear(true);
							clearObjectManagerLists(target);
							EditorUtility.SetDirty(target);
						}
					}

					m_TimelineIndex = 0;
					m_TimelineID = 0;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void drawProperty(SerializedProperty _Property, string _Label)
		{
			EditorGUI.showMixedValue = _Property.hasMultipleDifferentValues;
			EditorGUILayout.BeginHorizontal();
			{
				var style = _Property.isInstantiatedPrefab && _Property.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
				EditorGUILayout.LabelField(_Label, style);
				EditorGUILayout.PropertyField(_Property, new GUIContent(""));
			}
			EditorGUILayout.EndHorizontal();
			EditorGUI.showMixedValue = false;
		}

		private void clearObjectManagerLists(GAFMovieClip movieClip)
		{
			var objectManagers = (GAFObjectsManagerEditor[])Resources.FindObjectsOfTypeAll<GAFObjectsManagerEditor>();

			if (objectManagers != null && objectManagers.Length > 0)
			{
				for (int i = 0; i < objectManagers.Length; i++)
				{
					if (movieClip.manager == objectManagers[i].target)
					{
						objectManagers[i].clearLists();
						break;
					}
				}
			}
		}

		private void reloadTargets()
		{
			foreach (var target in targets)
			{
				if (target.isInitialized && PrefabUtility.GetPrefabType(target.gameObject) != PrefabType.Prefab)
				{
					target.reload();
					EditorUtility.SetDirty(target);
				}
			}
		}

		private bool hasPrefabs()
		{
			bool hasPrefabs = false;
			foreach (var target in targets)
			{
				if (PrefabUtility.GetPrefabType(target.gameObject) == PrefabType.Prefab)
				{
					hasPrefabs = true;
					break;
				}
			}

			return hasPrefabs;
		}

		private List<string> getSortingLayerNames()
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return ((string[])sortingLayersProperty.GetValue(null, new object[0])).ToList();
		}

		private List<int> getSortingLayerUniqueIDs()
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return ((int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0])).ToList();
		}

		#endregion // Implementation
	}
}