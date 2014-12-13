/*
 * File:			gafobjectimplsfactory.cs
 * Version:			3.10
 * Last changed:	2014/10/20 13:37
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

namespace GAF.Objects
{
	public static class GAFObjectImplsFactory
	{
		public static GAFObjectImpl getImpl(GameObject _Object, GAFObjectData _Data, Renderer _Renderer, MeshFilter _Filter)
		{
			switch (_Data.type)
			{
				case ObjectType.Simple: return new GAFObjectImpl(_Object, _Data, _Renderer, _Filter);
				case ObjectType.Masked: return new GAFMaskedObjectImpl(_Object, _Data, _Renderer, _Filter);
				case ObjectType.Mask:	return new GAFMaskObjectImpl(_Object, _Data, _Renderer, _Filter);
			}

			return null;
		}
	}
}
