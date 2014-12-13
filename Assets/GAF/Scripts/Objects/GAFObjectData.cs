/*
 * File:			GAFObjectData.cs
 * Version:			3.10
 * Last changed:	2014/10/20 13:38
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;

namespace GAF.Objects
{
	[System.Serializable]
	public class GAFObjectData : IGAFObjectSerializedProperties
	{
		#region Members

		[HideInInspector][SerializeField] private string			m_Name				= string.Empty;
		[HideInInspector][SerializeField] private ObjectType		m_Type				= ObjectType.Simple;
		[HideInInspector][SerializeField] private GAFMovieClip  	m_Clip 				= null;
		[HideInInspector][SerializeField] private GAFObjectsManager	m_Manager			= null;
		[HideInInspector][SerializeField] private int				m_ID 				= -1;
		[HideInInspector][SerializeField] private int				m_AtlasElementID	= -1;
		
		[HideInInspector][SerializeField] private bool				m_IsVisible			= true;
		[HideInInspector][SerializeField] private Material			m_Material			= null;
		[HideInInspector][SerializeField] private Vector3			m_StatePosition		= Vector3.zero;
		[HideInInspector][SerializeField] private Vector2			m_Offset			= Vector2.zero;

		#endregion // Members

		#region Interface

		public GAFObjectData(string _Name, ObjectType _Type, GAFMovieClip _Clip, GAFObjectsManager _Manager, uint _ID, uint _AtlasElementID)
		{
			m_Name				= _Name;
			m_Type				= _Type;
			m_Clip 				= _Clip;
			m_Manager			= _Manager;
			m_ID 				= (int)_ID;
			m_AtlasElementID	= (int)_AtlasElementID;
		}

		public GAFObjectData(GAFObjectData _Other)
		{
			m_Name				= _Other.name;
			m_Type				= _Other.type;
			m_Clip 				= _Other.clip;
			m_Manager			= _Other.manager;
			m_ID 				= (int)_Other.objectID;
			m_AtlasElementID	= (int)_Other.atlasElementID;
			m_IsVisible			= _Other.visible;
			m_Material			= _Other.material;
			m_StatePosition		= _Other.statePosition;
			m_Offset			= _Other.offset;
		}

		#endregion // Interface

		#region IGAFObjectSerializedProperties

		public string name
		{
			get
			{
				return m_Name;
			}
		}

		public ObjectType type
		{
			get
			{
				return m_Type;
			}
		}

		public uint objectID
		{
			get
			{
				return (uint)m_ID;
			}
		}

		public uint atlasElementID
		{
			get
			{
				return (uint)m_AtlasElementID;
			}
		}

		public GAFMovieClip clip
		{
			get
			{
				return m_Clip;
			}
		}

		public GAFObjectsManager manager
		{
			get
			{
				return m_Manager;
			}
		}

		public Vector3 localPosition 
		{
			get
			{
				return statePosition + (Vector3)offset;
			}
		}

		public Vector3 statePosition 
		{
			get
			{
				return m_StatePosition;
			}

			set
			{
				m_StatePosition = value;
			}
		}

		public bool visible
		{
			get
			{
				return m_IsVisible;
			}

			set
			{
				m_IsVisible = value;
			}
		}

		public Vector2 offset
		{
			get
			{
				return m_Offset;
			}

			set
			{
				m_Offset = value;
			}
		}

		public Material material
		{
			get
			{
				return m_Material;
			}

			set
			{
				m_Material = value;
			}
		}

		#endregion // IGAFObjectSerializedProperties
	}
}
