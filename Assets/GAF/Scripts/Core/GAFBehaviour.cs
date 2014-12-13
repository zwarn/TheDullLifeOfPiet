/*
 * File:			gafbehaviour.cs
 * Version:			3.10
 * Last changed:	2014/10/20 13:37
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

namespace GAF.Core
{
	[AddComponentMenu("")]
	public class GAFBehaviour : MonoBehaviour
	{
		private Transform _cachedTransform = null;

		public Transform cachedTransform
		{
			get
			{
				if (!_cachedTransform)
				{
					_cachedTransform = base.transform;
				}

				return _cachedTransform;
			}
		}
	}
}
