/*
 * File:			GAFObjectEditor.cs
 * Version:			3.9
 * Last changed:	2014/10/2 11:41
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEditor;
using UnityEngine;

using GAF.Objects;

namespace GAFEditor.Objects
{
	[CustomEditor(typeof(GAFObject))]
	public class GAFObjectEditor : Editor
	{
		new public GAFObject target
		{
			get
			{
				return (GAFObject)base.target;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(3f);
			var offset = EditorGUILayout.Vector2Field("Position offset: ", target.serializedProperties.offset);
			if (offset != target.serializedProperties.offset)
			{
				target.serializedProperties.offset = offset;
				target.transform.localPosition = target.serializedProperties.localPosition;
			}

			GUILayout.Space(2f);
			var visible = EditorGUILayout.Toggle("Visible: ", target.serializedProperties.visible);
			if (visible != target.serializedProperties.visible)
			{
				target.serializedProperties.visible = visible;
				target.serializedProperties.clip.reload();
			}

			GUILayout.Space(3f);
			EditorGUILayout.LabelField("* Custom material will break dynamic batching!");
			var material = EditorGUILayout.ObjectField("Custom material: ", target.serializedProperties.material, typeof(Material), false) as Material;
			if (material != target.serializedProperties.material)
			{
				target.serializedProperties.material = material;
				target.serializedProperties.clip.reload();
			}
		}

		private void OnEnable()
		{
			EditorApplication.update += OnUpdate;
		}

		private void OnUpdate()
		{
			if (target != null)
			{
				if (target.transform.localPosition != target.serializedProperties.localPosition)
				{
					target.serializedProperties.offset = (Vector2)(target.transform.localPosition - target.serializedProperties.statePosition);
				}
			}
			else
			{
				EditorApplication.update -= OnUpdate;
			}
		}
	}
}
