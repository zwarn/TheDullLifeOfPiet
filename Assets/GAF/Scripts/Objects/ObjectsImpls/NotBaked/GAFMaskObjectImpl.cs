/*
 * File:			GAFMaskObjectImpl.cs
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
	public class GAFMaskObjectImpl : GAFObjectImpl
	{
		#region Members

		private Texture2D m_MaskTexture = null;

		#endregion // Members

		#region Interface

		public GAFMaskObjectImpl(
			  GameObject		_ThisObject
			, GAFObjectData		_Data
			, Renderer			_Renderer
			, MeshFilter		_Filter)
			: base(_ThisObject, _Data, _Renderer, _Filter)
		{
			initMaskTexture();
		}

		public override void updateToState(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
			currentState = _State;

			var clip = serializedProperties.clip;
			float scale = clip.settings.pixelsPerUnit / clip.settings.scale;
			serializedProperties.statePosition = new Vector3(_State.tX / scale, -_State.tY / scale, -_State.zOrder);
			thisObject.transform.localPosition = serializedProperties.localPosition;
		}

		public override Texture2D texture
		{
			get
			{
				return m_MaskTexture;
			}
		}

		#endregion // Interface

		#region Implementation

		protected override void resetRenderer()
		{
			renderer.enabled = false;
		}

		protected override void resetMesh()
		{
			// Empty
		}

		private void initMaskTexture()
		{
			int csf = (int)serializedProperties.clip.settings.csf;

			m_MaskTexture = new Texture2D(
				  (int)(atlasElementData.width * csf)
				, (int)(atlasElementData.height * csf)
				, TextureFormat.ARGB32
				, false);

			Color[] textureColor = m_MaskTexture.GetPixels();
			for (uint i = 0; i < textureColor.Length; ++i)
				textureColor[i] = Color.black;

			m_MaskTexture.SetPixels(textureColor);
			m_MaskTexture.Apply();

			Texture2D atlasTexture = serializedProperties.clip.resource.getTexture(System.IO.Path.GetFileNameWithoutExtension(texturesData.getFileName(csf)));
			Color[] maskTexturePixels = atlasTexture.GetPixels(
				  (int)(atlasElementData.x * csf)
				, (int)(atlasTexture.height - atlasElementData.y * csf - atlasElementData.height * csf)
				, (int)(atlasElementData.width * csf)
				, (int)(atlasElementData.height * csf));

			m_MaskTexture.SetPixels(
				  0
				, 0
				, (int)(atlasElementData.width * csf)
				, (int)(atlasElementData.height * csf)
				, maskTexturePixels);

			m_MaskTexture.Apply(true);

			m_MaskTexture.filterMode = FilterMode.Bilinear;
			m_MaskTexture.wrapMode = TextureWrapMode.Clamp;

			m_MaskTexture.Apply();
		}

		#endregion // Implementation
	}
}