/*
 * File:			GAFBakedObjectImpl.cs
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
	public class GAFBakedObjectImpl : IGAFObjectImpl
	{
		#region Interface

		public GAFBakedObjectImpl(
			  IGAFObjectSerializedProperties	_Data
			, Renderer							_Renderer
			, MeshFilter						_Filter)
			: base(_Data, _Renderer, _Filter)
		{
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

		#region Implementation

		protected virtual void updateColor(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
			if (_Refresh ||
				currentState.alpha != _State.alpha)
			{
				if (!serializedProperties.visible)
				{
					if (_Processor.objectsToRender.ContainsKey(serializedProperties.objectID))
					{
						_Processor.state |= GAFRenderProcessor.MeshState.VertexSet;
						_Processor.objectsToRender.Remove(serializedProperties.objectID);
					}
				}
				else if (_State.alpha == 0f)
				{
					_Processor.state |= GAFRenderProcessor.MeshState.VertexSet;
					_Processor.objectsToRender.Remove(serializedProperties.objectID);
				}
				else
				{
					for (int i = 0; i < colors.Length; ++i)
						colors[i].a = (byte)(_State.alpha * 255f);

					if (!_Processor.objectsToRender.ContainsKey(serializedProperties.objectID))
					{
						_Processor.state |= GAFRenderProcessor.MeshState.VertexSet;
						_Processor.objectsToRender.Add(serializedProperties.objectID, serializedProperties.clip.getObject(serializedProperties.objectID));
					}
					else
					{
						_Processor.state |= GAFRenderProcessor.MeshState.VertexChange;
					}
				}

				currentState.alpha = _State.alpha;
			}

			if (_Refresh ||
				currentState.colorMatrix != _State.colorMatrix)
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

				if (serializedProperties.visible)
				{
					_Processor.state |= GAFRenderProcessor.MeshState.VertexChange;
				}

				currentState.colorMatrix = _State.colorMatrix;
			}
		}

		protected virtual void updateTransform(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
			if (currentState.alpha > 0)
			{
				if (_Refresh ||
					currentState.a != _State.a ||
					currentState.b != _State.b ||
					currentState.c != _State.c ||
					currentState.d != _State.d ||
					currentState.tX != _State.tX ||
					currentState.tY != _State.tY ||
					currentState.zOrder != _State.zOrder)
				{
					var clip = serializedProperties.clip;

					if (currentState.zOrder != _State.zOrder)
					{
						_Processor.state |= GAFRenderProcessor.MeshState.VertexSet;
					}
					else
					{
						_Processor.state |= GAFRenderProcessor.MeshState.VertexChange;
					}

					_Processor.objectsToRender[serializedProperties.objectID] = clip.getObject(serializedProperties.objectID);

					float scale = clip.settings.pixelsPerUnit / clip.settings.scale;

					Matrix4x4 _transform = Matrix4x4.identity;
					_transform[0, 0] = _State.a;
					_transform[0, 1] = -_State.c;
					_transform[1, 0] = -_State.b;
					_transform[1, 1] = _State.d;
					_transform[0, 3] = _State.tX / scale  + serializedProperties.offset.x;
					_transform[1, 3] = -_State.tY / scale + serializedProperties.offset.y;
					_transform[2, 3] = _State.zOrder;

					serializedProperties.statePosition = new Vector3(_State.tX / scale, -_State.tY / scale, _State.zOrder);

					for (int i = 0; i < initialVertices.Length; i++)
					{
						currentVertices[i] = _transform.MultiplyPoint3x4(initialVertices[i]);
					}

					currentState.tX		= _State.tY;
					currentState.tY		= _State.tY;
					currentState.zOrder = _State.zOrder;
					currentState.a		= _State.a;
					currentState.b		= _State.b;
					currentState.c		= _State.c;
					currentState.d		= _State.d;
				}
			}
		}

		#endregion // Implementation
	}
}
