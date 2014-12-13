/*
 * File:			gafrenderprocessor.cs
 * Version:			3.10
 * Last changed:	2014/10/20 13:38
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;

using System.Collections.Generic;
using System.Linq;

namespace GAF.Objects
{
	public class GAFRenderProcessor
	{
		#region Enums

		[System.Flags]
		public enum MeshState
		{
			  Null			= 0
			, VertexChange	= 1
			, VertexSet		= 2
		}

		#endregion // Enums

		#region Members

		private static readonly Vector3 normalVector = new Vector3(0, 0, -1f);

		private MeshFilter		m_Filter	= null;
		private Renderer		m_Renderer	= null;
		private GAFMovieClip	m_Clip		= null;

		#endregion // Members

		#region Interface

		public GAFRenderProcessor(GAFMovieClip _Clip, MeshFilter _Filter, Renderer _Renderer)
		{
			m_Filter	= _Filter;
			m_Renderer	= _Renderer;
			m_Clip		= _Clip;

			state = MeshState.Null;
			objectsToRender = new Dictionary<uint, IGAFObject>();

			m_Renderer.castShadows		= false;
			m_Renderer.receiveShadows	= false;
			m_Renderer.sortingLayerID	= m_Clip.settings.spriteLayerID;
			m_Renderer.sortingOrder		= m_Clip.settings.spriteLayerValue;
		}

		public void process()
		{			
			if (isStateSet(MeshState.VertexSet))
			{
				resetMesh();
			}
			else if (isStateSet(MeshState.VertexChange))
			{
				changeMesh();
			}

			state = MeshState.Null;
		}

		#endregion // Interface

		#region Properties

		public MeshFilter filter
		{
			get
			{
				return m_Filter;
			}
		}

		public Renderer renderer
		{
			get
			{
				return m_Renderer;
			}
		}

		public Dictionary<uint, IGAFObject> objectsToRender
		{
			get;
			private set;
		}

		public MeshState state
		{
			get;
			set;
		}

		#endregion // Properties

		#region Implementation

		private List<IGAFObject> sortedObjects
		{
			get;
			set;
		}

		private bool isStateSet(MeshState _State)
		{
			return ((state & _State) == _State);
		}

		private void resetMesh()
		{
			sortedObjects = objectsToRender.Values.ToList();
			sortedObjects.Sort();

			m_Filter.sharedMesh.Clear();

			int capacity = sortedObjects.Count;

 			Vector3[]	vertices	= new Vector3[capacity * 4];
			Vector2[]	uvs			= new Vector2[capacity * 4];
			Color32[]	colors		= new Color32[capacity * 4];
			Vector4[]	tangents	= new Vector4[capacity * 4];
			List<int[]> triangles	= new List<int[]>();
			Material[]	materials	= new Material[capacity];
			Vector3[]	normals		= new Vector3[capacity * 4];

			for (int i = 0; i < normals.Length; i++)
			{
				normals[i] = normalVector;
			}

			m_Filter.sharedMesh.subMeshCount = capacity;

			int index = 0;
			int materialIndex = 0;
			foreach (var obj in sortedObjects)
			{
				obj.properties.currentVertices.CopyTo(vertices, index);
				obj.properties.uvs.CopyTo(uvs, index);
				obj.properties.colors.CopyTo(colors, index);
				obj.properties.colorsShift.CopyTo(tangents, index);

				materials[materialIndex] = obj.properties.currentMaterial;

				triangles.Add(new int[]
				{
 					  2 + index
					, 0 + index
					, 1 + index
					, 3 + index
					, 0 + index
					, 2 + index
				});

				++materialIndex;
				index += 4;
			}

			m_Filter.sharedMesh.MarkDynamic();

			m_Filter.sharedMesh.vertices = vertices;
			m_Filter.sharedMesh.uv = uvs;
			m_Filter.sharedMesh.normals = normals;
			m_Filter.sharedMesh.colors32 = colors;
			m_Filter.sharedMesh.tangents = tangents;

			for (int i = 0; i < triangles.Count; i++)
			{
				m_Filter.sharedMesh.SetTriangles(triangles[i], i);
			}

			m_Renderer.sharedMaterials = materials;
		}

		private void changeMesh()
		{
			int capacity = sortedObjects.Count;
 			Vector3[] vertices = new Vector3[capacity * 4];
			Color32[] colors = new Color32[capacity * 4];
			Vector4[] tangents = new Vector4[capacity * 4];

			int index = 0;
			foreach (var obj in sortedObjects)
			{
				obj.properties.currentVertices.CopyTo(vertices, index);
				obj.properties.colors.CopyTo(colors, index);
				obj.properties.colorsShift.CopyTo(tangents, index);
				index += 4;
			}

			m_Filter.sharedMesh.MarkDynamic();

			m_Filter.sharedMesh.vertices = vertices;
			m_Filter.sharedMesh.colors32 = colors;
			m_Filter.sharedMesh.tangents = tangents;
			m_Filter.sharedMesh.RecalculateBounds();
		}

		#endregion // Implementation
	}
}