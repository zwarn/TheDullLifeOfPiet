/*
 * File:			gafobjectsmanagereditor.cs
 * Version:			3.9
 * Last changed:	2014/10/3 12:56
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using GAF.Core;

namespace GAFEditor.Core
{
	[CustomEditor(typeof(GAFObjectsManager))]
	public class GAFObjectsManagerEditor : Editor
	{
		private bool m_ShowObjects = true;
		private List<uint> m_WithoutController = null;
		private List<uint> m_WithContoller = null;
		private Vector2 m_ScrollPosition = new Vector2();

		#region Interface

		new public GAFObjectsManager target
		{
			get { return base.target as GAFObjectsManager; }
		}

		public void clearLists()
		{
			if (m_WithContoller != null)
			{
				m_WithContoller.Clear();
				m_WithContoller = null;
			}
			if (m_WithoutController != null)
			{
				m_WithoutController.Clear();
				m_WithoutController = null;
			}
		}

		#endregion // Interface.

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(2f);

			var enabled = target.objects != null && target.objects.Count() > 0 && target.objects.All(obj => !System.Object.Equals(obj, null));

			if (!enabled)
			{
				EditorGUILayout.LabelField("It's no baked objects in movie clip", EditorStyles.boldLabel);
			}
			else
			{
				if (m_WithoutController == null)
				{
					m_WithoutController = target.objects.Where((obj) => !target.hasController(obj)).Select(obj => obj.serializedProperties.objectID).ToList();
					m_WithContoller = target.objects.Where((obj) => target.hasController(obj)).Select(obj => obj.serializedProperties.objectID).ToList();
				}

				drawSplitter(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
				var oldMode = !EditorGUILayout.Toggle("Bake into a sigle mesh", !target.oldMode);
				drawSplitter(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
				GUILayout.Space(4f);

				if (oldMode) GUI.enabled = false;

				if (target.oldMode != oldMode)
				{
					if (oldMode)
					{
						target.regroupInOldMode();
						target.movieClip.reload();
					}
					else
					{
						target.regroupInNewMode();
						target.movieClip.reload();
					}

					refillControllersLists();
				}

				m_ShowObjects = EditorGUILayout.Foldout(m_ShowObjects, "Objects: ");

				if (m_ShowObjects)
				{
					EditorGUILayout.BeginVertical();
					{
						var horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
						var verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
						var area = new GUIStyle(GUI.skin.textArea);

						m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false, horizontalScrollbar, verticalScrollbar, area, GUILayout.Height(200f));
						{
							var selectedAllValue = target.objectsDict.Count == m_WithoutController.Count;
							var actualState = selectedAllValue;

							actualState = EditorGUILayout.ToggleLeft("\tAll", selectedAllValue, EditorStyles.boldLabel);

							GUILayout.Space(5f);

							if (selectedAllValue != actualState)
							{
								if (actualState)
								{
									m_WithoutController = m_WithoutController.Union(m_WithContoller).ToList();
									m_WithContoller.Clear();
								}
								else if (selectedAllValue)
								{
									m_WithContoller = m_WithContoller.Union(m_WithoutController).ToList();
									m_WithoutController.Clear();
								}
							}
							foreach (var obj in target.objectsDict.Values)
							{
								EditorGUILayout.BeginHorizontal();
								{
									var currentEnabled = m_WithoutController.Contains(obj.serializedProperties.objectID);
									var nextEnabled = EditorGUILayout.ToggleLeft("\t" + obj.serializedProperties.name, currentEnabled);

									if (nextEnabled != currentEnabled)
									{
										if (nextEnabled)
										{
											m_WithoutController.Add(obj.serializedProperties.objectID);
											m_WithContoller.Remove(obj.serializedProperties.objectID);
										}
										else
										{
											m_WithContoller.Add(obj.serializedProperties.objectID);
											m_WithoutController.Remove(obj.serializedProperties.objectID);
										}
									}
								}
								EditorGUILayout.EndHorizontal();
								GUILayout.Space(1f);
							}
						}
						EditorGUILayout.EndScrollView();

						GUILayout.Space(5f);

						var actualWithout = getObjectsWithoutController();
						var actualWith = getObjectsWithController();

						GUI.enabled = actualWithout.ConvertAll(value => (int)value).Sum() != m_WithoutController.ConvertAll(value => (int)value).Sum();

						EditorGUILayout.BeginHorizontal();
						{
							if (GUILayout.Button("Commit"))
							{
								var toRemove = m_WithoutController.Except(actualWithout).ToList();
								var toAdd = m_WithContoller.Except(actualWith).ToList();

								for (int i = 0; i < toAdd.Count; i++)
								{
									target.addController(toAdd[i]);
								}
								for (int i = 0; i < toRemove.Count; i++)
								{
									target.removeController(toRemove[i]);
								}

								target.movieClip.reload();

								refillControllersLists();
							}

							if (GUILayout.Button("Cancel"))
							{
								refillControllersLists();
							}
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}

				GUILayout.Space(5f);
			}
		}

		private void refillControllersLists()
		{
			m_WithoutController = getObjectsWithoutController();
			m_WithContoller = getObjectsWithController();
		}

		private List<uint> getObjectsWithoutController()
		{
			return target.objects.Where((obj) => !target.hasController(obj)).Select(obj => obj.serializedProperties.objectID).ToList();
		}

		private List<uint> getObjectsWithController()
		{
			return target.objects.Where((obj) => target.hasController(obj)).Select(obj => obj.serializedProperties.objectID).ToList();
		}

		private void drawSplitter(Color _Color, float _Thickness)
		{
			var splitter = new GUIStyle();
			splitter.normal.background = EditorGUIUtility.whiteTexture;
			splitter.stretchWidth = true;
			splitter.margin = new RectOffset(0, 0, 1, 1);

			Rect splitterRect = GUILayoutUtility.GetRect(GUIContent.none, splitter, GUILayout.Height(_Thickness));
			if (Event.current.type == EventType.Repaint)
			{
				Color restoreColor = GUI.color;
				GUI.color = _Color;
				splitter.Draw(splitterRect, false, false, false, false);
				GUI.color = restoreColor;
			}
		}
	}
}