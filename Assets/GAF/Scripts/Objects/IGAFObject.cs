/*
 * File:			IGAFObject.cs
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
	public interface IGAFObject : System.IComparable
	{
		#region Base Methods

		void initialize(string _Name, ObjectType _Type, GAFMovieClip _Player, GAFObjectsManager _Manager, uint _ObjectID, uint _AtlasElementID);

		void reload(GAFRenderProcessor _Processor);

		void updateToState(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool Refresh);

		void onDestroy();

		#endregion // Base Methods

		#region Properties

		IGAFObjectProperties properties { get; }

		IGAFObjectSerializedProperties serializedProperties { get; }

		#endregion // Properties
	}
}