/*
 * File:			gafobjectimpl.cs
 * Version:			3.10
 * Last changed:	2014/10/20 13:37
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public class GAFObjectImpl : IGAFObjectImpl
	{
		#region Members

		private GameObject m_Object = null;

		#endregion // Members

		#region Interface

		public GAFObjectImpl(
			  GameObject		_ThisObject
			, GAFObjectData		_Data
			, Renderer			_Renderer
			, MeshFilter		_Filter) : base(_Data, _Renderer, _Filter)
		{
			m_Object = _ThisObject;

			resetRenderer();
			resetMesh();
		}

		public override void updateToState(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
			updateColor(_State, _Processor, _Refresh);
			updateTransform(_State, _Processor, _Refresh);
		}

		public virtual void onWillRenderObject()
		{ 
		}

		#endregion // Interface

		#region Protected properties

		protected GameObject thisObject
		{
			get
			{
				return m_Object;
			}
		}

		#endregion // Protected properties

		#region Implementation

		protected virtual void updateColor(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
			var setColors		= false;
			var setColorsShift	= false;

			if (currentState.alpha != _State.alpha ||
				_Refresh)
			{
				if (_State.alpha == 0f)
				{
					renderer.enabled = false;
				}
				else
				{
					renderer.enabled = serializedProperties.visible;

					for (int i = 0; i < colors.Length; ++i)
						colors[i].a = (byte)(_State.alpha * 255f);
				}

				setColors = true;

				currentState.alpha = _State.alpha;
			}

			if (currentState.colorMatrix != _State.colorMatrix ||
				_Refresh)
			{
				if (_State.colorMatrix != null)
				{
					for (int i = 0; i < colors.Length; ++i)
						colors[i] = _State.colorMatrix.multipliers;

					for (int i = 0; i < colorsShift.Length; ++i)
						colorsShift[i] = _State.colorMatrix.offsets;
				}
				else
				{
					for (int i = 0; i < colors.Length; ++i)
					{
						colors[i].r = (byte)255;
						colors[i].g = (byte)255;
						colors[i].b = (byte)255;
					}

					var offset = new Vector4(0f, 0f, 0f, 0f);
					for (int i = 0; i < colorsShift.Length; ++i)
						colorsShift[i] = offset;
				}

				setColors = true;
				setColorsShift = true;

				currentState.colorMatrix = _State.colorMatrix;
			}

			if (setColors)
				filter.sharedMesh.colors32 = colors;

			if (setColorsShift)
				filter.sharedMesh.tangents = colorsShift;
		}

		protected virtual void updateTransform(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
			if (currentState.alpha > 0)
			{
				if (_Refresh ||
					currentState.tX != _State.tX ||
					currentState.tY != _State.tY ||
					currentState.zOrder != _State.zOrder)
				{
					var clip = serializedProperties.clip;
					float scale = clip.settings.pixelsPerUnit / clip.settings.scale;

					serializedProperties.statePosition = new Vector3((_State.tX) / scale, (-_State.tY) / scale, _State.zOrder);
					m_Object.transform.localPosition = serializedProperties.localPosition;

					currentState.tX = _State.tX;
					currentState.tY = _State.tY;
					currentState.zOrder = _State.zOrder;
				}

				if (_Refresh ||
					currentState.a != _State.a ||
					currentState.b != _State.b ||
					currentState.c != _State.c ||
					currentState.d != _State.d)
				{
					Matrix4x4 _transform = Matrix4x4.identity;
					_transform[0, 0] = _State.a;
					_transform[0, 1] = -_State.c;
					_transform[1, 0] = -_State.b;
					_transform[1, 1] = _State.d;

					for (int i = 0; i < currentVertices.Length; i++)
						currentVertices[i] = _transform * initialVertices[i];

					filter.sharedMesh.vertices = currentVertices;
					filter.sharedMesh.RecalculateBounds();

					currentState.a = _State.a;
					currentState.b = _State.b;
					currentState.c = _State.c;
					currentState.d = _State.d;
				}
			}
		}

		protected virtual void resetRenderer()
		{
			var clip = serializedProperties.clip;

			renderer.sharedMaterial	= currentMaterial;
			renderer.castShadows	= false;
			renderer.receiveShadows = false;
			renderer.sortingLayerID = clip.settings.spriteLayerID;
			renderer.sortingOrder	= clip.settings.spriteLayerValue;
		}

		protected virtual void resetMesh()
		{
			Mesh mesh = new Mesh();
			mesh.name = serializedProperties.name;

			mesh.vertices	= initialVertices;
			mesh.uv			= uvs;
			mesh.triangles	= triangles;
			mesh.normals	= normals;
			mesh.colors32	= colors;

			filter.mesh = mesh;
		}

		#endregion // Implementation
	}
}
