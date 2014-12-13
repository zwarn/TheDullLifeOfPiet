/*
 * File:			GAFBakedObject.cs
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
	[System.Serializable]
	public class GAFBakedObject : IGAFObject
	{
		#region Members
		
		[HideInInspector][SerializeField] private GAFObjectData				m_Data			= null;
		[HideInInspector][SerializeField] private GAFBakedObjectController	m_Controller	= null;

		[HideInInspector]
		[System.NonSerialized]
		private GAFBakedObjectImpl m_Impl = null;

		#endregion // Members

		#region Base Methods Impl

		public void initialize(string _Name, ObjectType _Type, GAFMovieClip _Clip, GAFObjectsManager _Manager, uint _ObjectID, uint _AtlasElementID)
		{
			m_Data = new GAFObjectData(_Name, _Type, _Clip, _Manager, _ObjectID, _AtlasElementID);
		}

		public void reload(GAFRenderProcessor _Processor)
		{
			if (m_Controller != null)
				m_Controller.registerObject(this);

			m_Impl = GAFBakedObjectImplsFactory.getImpl(m_Data, _Processor.renderer, _Processor.filter);

			if (m_Data.type == ObjectType.Masked)
				serializedProperties.manager.onWillRenderObject += m_Impl.onWillRenderObject;
		}

		public void updateToState(GAFObjectStateData _State, GAFRenderProcessor _Processor, bool _Refresh)
		{
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

		#region Baked Object Interface

		public void setController(GAFBakedObjectController _Controller)
		{
			m_Controller = _Controller;

			if (m_Controller != null)
			{
				m_Controller.registerObject(this);
			}
		}

		#endregion // Baked Object Interface

		#region IComparable

		public int CompareTo(object other)
		{
			return properties.zOrder.CompareTo(((IGAFObject)other).properties.zOrder);
		}

		#endregion // IComparable
	}
}