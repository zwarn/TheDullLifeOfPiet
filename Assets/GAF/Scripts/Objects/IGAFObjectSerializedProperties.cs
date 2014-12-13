/*
 * File:			IGAFObjectSerializedProperties.cs
 * Version:			3.10
 * Last changed:	2014/10/20 13:38
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public enum ObjectType
	{
		  Simple
		, Mask
		, Masked
	}

	public interface IGAFObjectSerializedProperties
	{
		string name { get; }

		ObjectType type { get; }

		uint objectID { get; }

		uint atlasElementID { get; }

		GAFMovieClip clip { get; }

		GAFObjectsManager manager { get; }

		Vector3 localPosition { get; }

		Vector3 statePosition { get; set; }

		bool visible { get; set; }

		Vector2 offset { get; set; }

		Material material { get; set; }
	}
}
