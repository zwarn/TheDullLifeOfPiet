/*
 * File:			GAFBakedObjectControllerEditor.cs
 * Version:			3.9
 * Last changed:	2014/10/2 11:48
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEditor;
using UnityEngine;

using GAF.Objects;

namespace GAFEditor.Objects
{
	[CustomEditor(typeof(GAFBakedObjectController))]
	public class GAFBakedObjectControllerEditor : Editor
	{
		#region Properties

		new public GAFBakedObjectController target
		{
			get
			{
				return base.target as GAFBakedObjectController;
			}
		}

		#endregion // Properties

		private void OnEnable()
		{
			EditorApplication.update += OnUpdate;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(3f);
			var offset = EditorGUILayout.Vector2Field("Position offset: ", target.bakedObject.serializedProperties.offset);
			if (offset != target.bakedObject.serializedProperties.offset)
			{
				target.bakedObject.serializedProperties.offset = offset;
				target.transform.localPosition = target.bakedObject.serializedProperties.localPosition;
				target.bakedObject.serializedProperties.clip.reload();
			}

			GUILayout.Space(2f);
			var visible = EditorGUILayout.Toggle("Visible: ", target.bakedObject.serializedProperties.visible);
			if (visible != target.bakedObject.serializedProperties.visible)
			{
				target.bakedObject.serializedProperties.visible = visible;
				target.bakedObject.serializedProperties.clip.reload();
			}

			GUILayout.Space(3f);
			EditorGUILayout.LabelField("* Custom material will break dynamic batching!");
			var material = EditorGUILayout.ObjectField("Custom material: ", target.bakedObject.serializedProperties.material, typeof(Material), false) as Material;
			if (material != target.bakedObject.serializedProperties.material)
			{
				target.bakedObject.serializedProperties.material = material;
				target.bakedObject.serializedProperties.clip.reload();
			}

			GUILayout.Space(5f);
			if (GUILayout.Button(new GUIContent("Copy mesh")))
			{
				target.copyMesh();
			}
		}

		private void OnUpdate()
		{
			if (target != null)
			{
				if (target.transform.localPosition != target.bakedObject.serializedProperties.localPosition)
				{
					target.bakedObject.serializedProperties.offset = (Vector2)(target.transform.localPosition - target.bakedObject.serializedProperties.statePosition);
					target.bakedObject.serializedProperties.clip.reload();
				}
			}
			else
			{
				EditorApplication.update -= OnUpdate;
			}
		}
	}
}
