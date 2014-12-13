/*
 * File:			gafbakedmaskedobjectimpl.cs
 * Version:			3.10
 * Last changed:	2014/10/20 18:09
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

//#define GAF_USING_TK2D

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public class GAFBakedMaskedObjectImpl : GAFBakedObjectImpl
	{
		#region Members

		private int m_MaskID = -1;

		#endregion // Members

		#region Interface

		public GAFBakedMaskedObjectImpl(
			  GAFObjectData		_Data
			, Renderer			_Renderer
			, MeshFilter		_Filter)
			: base(_Data, _Renderer, _Filter)
		{
		}

		public override void onWillRenderObject()
		{
			var clip = serializedProperties.clip;

			if (clip != null &&
				clip.asset != null &&
				clip.asset.isLoaded &&
				clip.resource != null &&
				clip.resource.isReady &&
				material != null)
			{
				if (m_MaskID >= 0)
				{
					var mask = serializedProperties.manager.objectsDict[(uint)m_MaskID];
					if (mask != null &&
						mask.properties.currentState != null &&
						mask.properties.currentState.alpha > 0 &&
						mask.properties.texture != null &&
						mask.properties.atlasElementData != null &&
						mask.serializedProperties.visible)
					{
						applyMask(mask);
					}
					else
					{
						material.SetTexture("_MaskMap", null);
					}
				}
			}
		}

		#endregion // Interface

		#region Implementation

		protected override void updateColor(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
			if (_State.maskID != m_MaskID)
			{
				m_MaskID = _State.maskID;

				if (m_MaskID < 0)
				{
					var clip = serializedProperties.clip;
					material = clip.resource.getSharedMaterial(System.IO.Path.GetFileNameWithoutExtension(texturesData.getFileName(clip.settings.csf)));
					material.renderQueue = 3000;
				}
				else
				{
					material = new Material(Shader.Find("GAF/GAFMaskedObject"));
					material.color = new Color(1f, 1f, 1f, 1f);
					material.mainTexture = texture;
					material.renderQueue = 3000;
				}
			}

			if (m_MaskID < 0)
			{
				base.updateColor(_State, _Processor, _Refresh);
			}
			else
			{
				updateMaterialData(_State, _Processor, _Refresh);
			}
		}

		private void updateMaterialData(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
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

					var color = new Color(
						  (float)colors[0].r / 255f
						, (float)colors[0].g / 255f
						, (float)colors[0].b / 255f
						, (float)colors[0].a / 255f);

					material.SetColor("_Color", color);
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
					var color = new Color(
						  (float)colors[0].r / 255f
						, (float)colors[0].g / 255f
						, (float)colors[0].b / 255f
						, (float)colors[0].a / 255f);

					material.SetColor("_Color", color);
					material.SetVector("_Offset", colorsShift[0]);
				}

				currentState.colorMatrix = _State.colorMatrix;
			}
		}

		private void applyMask(IGAFObject _Mask)
		{
			Matrix4x4 maskTransform = Matrix4x4.identity;
			maskTransform.m00 = _Mask.properties.currentState.a;
			maskTransform.m01 = _Mask.properties.currentState.c;
			maskTransform.m10 = _Mask.properties.currentState.b;
			maskTransform.m11 = _Mask.properties.currentState.d;

#if GAF_USING_TK2D
			float screenHeight 		= 0;
			float screenWidth  		= 0;
			Vector2 cameraPosShift	= Vector2.zero;
		
			tk2dCamera tk2d_camera = Camera.current.GetComponent<tk2dCamera>();
			if (tk2d_camera != null)
			{
				tk2dCameraSettings cameraSettings = tk2d_camera.CameraSettings;
				if (cameraSettings.orthographicType == tk2dCameraSettings.OrthographicType.PixelsPerMeter)
					screenHeight = tk2d_camera.nativeResolutionHeight / cameraSettings.orthographicPixelsPerMeter;
				else
					screenHeight = tk2d_camera.CameraSettings.orthographicSize * 2;

				screenWidth  	= Camera.current.aspect * screenHeight;
				cameraPosShift	= Camera.current.transform.position - new Vector3(screenWidth / 2f, -screenHeight / 2f);
			}
			else
			{
				screenHeight 	= Camera.current.orthographicSize * 2;
				screenWidth  	= Camera.current.aspect * screenHeight;
				cameraPosShift	= Camera.current.transform.position - new Vector3(screenWidth / 2f, -screenHeight / 2f);
			}
#else
			float screenHeight = Camera.current.orthographicSize * 2;
			float screenWidth = Camera.current.aspect * screenHeight;
			Vector2 cameraPosShift = Camera.current.transform.position - new Vector3(screenWidth / 2f, -screenHeight / 2f);
#endif // GAF_USING_TK2D

			var clip = serializedProperties.clip;

			float scaleX = Mathf.Sqrt((maskTransform.m00 * maskTransform.m00) + (maskTransform.m01 * maskTransform.m01));
			float scaleY = Mathf.Sqrt((maskTransform.m11 * maskTransform.m11) + (maskTransform.m10 * maskTransform.m10));

			float scale = clip.settings.pixelsPerUnit * _Mask.properties.atlasElementData.scale * clip.settings.csf;
			float sizeXUV = (float)screenWidth / (_Mask.properties.texture.width / scale * scaleX * clip.transform.localScale.x * Camera.current.aspect);
			float sizeYUV = (float)screenHeight / (_Mask.properties.texture.height / scale * scaleY * clip.transform.localScale.y);

			float maskWidth = (float)_Mask.properties.texture.width / clip.settings.csf;
			float maskHeight = (float)_Mask.properties.texture.height / clip.settings.csf;

			float pivotX = _Mask.properties.atlasElementData.pivotX / maskWidth;
			float pivotY = (maskHeight - _Mask.properties.atlasElementData.pivotY) / maskHeight;

			float moveX = (-_Mask.serializedProperties.localPosition.x - clip.transform.position.x + cameraPosShift.x) / screenWidth;
			float moveY = -1f - (_Mask.serializedProperties.localPosition.y + clip.transform.position.y - cameraPosShift.y) / screenHeight;

			Matrix4x4 _transform = Matrix4x4.identity;
			_transform *= Matrix4x4.TRS(new Vector3(pivotX, pivotY, 0f), Quaternion.identity, Vector3.one);
			_transform *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(sizeXUV, sizeYUV, 1f));
			_transform *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -clip.transform.localRotation.eulerAngles.z), Vector3.one);
			_transform *= maskTransform;
			_transform *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f / scaleX, 1f / scaleY, 1f));
			_transform *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Camera.current.aspect, 1f, 1f));
			_transform *= Matrix4x4.TRS(new Vector3(moveX, moveY, 0f), Quaternion.identity, Vector3.one);

			material.SetMatrix("_TransformMatrix", _transform);
			material.SetTexture("_MaskMap", _Mask.properties.texture);
		}

		#endregion // Implementation
	}
}
