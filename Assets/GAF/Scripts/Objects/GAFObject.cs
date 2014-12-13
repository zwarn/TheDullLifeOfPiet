/*
 * File:			gafobject.cs
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
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class GAFObject : GAFBehaviour, IGAFObject
	{
		#region Members

		[HideInInspector][SerializeField] private GAFObjectData		m_Data				= null;
		[HideInInspector][SerializeField] private MeshFilter		m_Filter			= null;
		[HideInInspector][SerializeField] private MeshRenderer		m_MeshRenderer		= null;

		[HideInInspector]
		[System.NonSerialized]
		private GAFObjectImpl m_Impl = null;
	
		#endregion // Members

		#region Base Methods Impl

		public void initialize(string _Name, ObjectType _Type, GAFMovieClip _Clip, GAFObjectsManager _Manager, uint _ObjectID, uint _AtlasElementID)
		{
			m_Filter		= GetComponent<MeshFilter>();
			m_MeshRenderer	= GetComponent<MeshRenderer>();

			m_Data = new GAFObjectData(_Name, _Type, _Clip, _Manager, _ObjectID, _AtlasElementID);
		}

		public void reload(GAFRenderProcessor _Processor)
		{
			m_Impl = GAFObjectImplsFactory.getImpl(gameObject, m_Data, m_MeshRenderer, m_Filter);
		}

		public void updateToState(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
			cachedTransform.localRotation = Quaternion.identity;
			cachedTransform.localScale = Vector3.one;
			gameObject.SetActive(_State.alpha > 0);

			m_Impl.updateToState(_State, _Processor, _Refresh);
		}

		public void onDestroy()
		{
			m_Impl.onDestroy();
		}

		#endregion // Base Methods Impl

		#region Properties

		public IGAFObjectProperties properties
		{
			get
			{
				return m_Impl;
			}
		}

		public IGAFObjectSerializedProperties serializedProperties
		{
			get
			{
				return m_Data;
			}
		}

		#endregion // Properties

		#region IComparable

		public int CompareTo(object other)
		{
			return properties.zOrder.CompareTo(((IGAFObject)other).properties.zOrder);
		}

		#endregion // IComparable

		#region MonoBehaviour

		private void OnWillRenderObject()
		{
			if (m_Data != null && m_Impl != null)
			{
				if (m_Data.type == ObjectType.Masked)
					m_Impl.onWillRenderObject();
			}
		}

		#endregion // MonoBehaviour
	}
}