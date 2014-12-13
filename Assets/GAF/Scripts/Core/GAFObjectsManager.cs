/*
 * File:			gafobjectsmanager.cs
 * Version:			3.10
 * Last changed:	2014/10/20 17:36
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using GAF.Objects;
using GAF.Utils;

namespace GAF.Core
{
	[System.Serializable]
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[DisallowMultipleComponent]
	public class GAFObjectsManager : GAFBehaviour
	{
		#region Events

		public event System.Action onWillRenderObject;

		#endregion // Events

		#region Members

		[HideInInspector][SerializeField] private GAFMovieClip						m_MovieClip		= null;
		[HideInInspector][SerializeField] private List<GAFBakedObject>				m_BakedObjects	= new List<GAFBakedObject>();
		[HideInInspector][SerializeField] private List<GAFObject>					m_Objects		= new List<GAFObject>();
		[HideInInspector][SerializeField] private List<GAFBakedObjectController>	m_Controllers	= new List<GAFBakedObjectController>();
		[HideInInspector][SerializeField] private MeshFilter						m_Filter		= null;
		[HideInInspector][SerializeField] private bool								m_OldMode		= false;

		private Dictionary<uint, IGAFObject>	m_AllObjects		= new Dictionary<uint, IGAFObject>();
		private GAFRenderProcessor				m_RenderProcessor	= null;
		 
		#endregion // Members

		#region Properties

		public IEnumerable<IGAFObject> objects
		{
			get
			{
				return oldMode ? m_Objects.Cast<IGAFObject>() : m_BakedObjects.Cast<IGAFObject>();
			}
		}

		public GAFMovieClip movieClip 
		{
			get
			{
				return m_MovieClip;
			}
		}

		public bool oldMode
		{
			get
			{
				return m_OldMode;
			}
		}

		public Dictionary<uint, IGAFObject> objectsDict
		{
			get
			{
				if (m_AllObjects == null || m_AllObjects.Count == 0)
				{
					m_AllObjects = new Dictionary<uint, IGAFObject>();
					foreach (var _object in m_Objects)
					{
						m_AllObjects[_object.serializedProperties.objectID] = _object;
					}

					foreach (var _object in m_BakedObjects)
					{
						m_AllObjects[_object.serializedProperties.objectID] = _object;
					}
				}

				return m_AllObjects;
			}
		}

		#endregion // Properties

		#region Interface

		public void initialize(GAFMovieClip _Player)
		{
			m_MovieClip = _Player;

			m_Filter = GetComponent<MeshFilter>();

			if (!oldMode)
			{
				createNewModeObjects();
			}
			else
			{
				createOldModeObjects();
			}
		}

		public void regroupInOldMode()
		{
			if (!m_OldMode)
			{
				if (m_RenderProcessor != null)
					m_RenderProcessor.objectsToRender.Clear();

				m_AllObjects = null;

				for (int i = 0; i < m_BakedObjects.Count; i++)
				{
					if (hasController(m_BakedObjects[i]))
						removeController(m_BakedObjects[i]);
				}

				m_BakedObjects.Clear();

				createOldModeObjects();

				m_OldMode = true;
			}
		}

		public void regroupInNewMode()
		{
			if (m_OldMode)
			{
				if (m_RenderProcessor != null)
					m_RenderProcessor.objectsToRender.Clear();

				m_AllObjects = null;

				for (int i = 0; i < m_Objects.Count; i++)
				{
					if (!Application.isPlaying)
					{
						DestroyImmediate(m_Objects[i].gameObject);
						DestroyImmediate(m_Objects[i]);
					}
					else
					{
						Destroy(m_Objects[i].gameObject);
						Destroy(m_Objects[i]);
					}
				}

				m_Objects.Clear();

				createNewModeObjects();

				m_OldMode = false;
			}
		}

		public void reload()
		{
			if (!m_OldMode)
			{
				m_RenderProcessor = new GAFRenderProcessor(m_MovieClip, m_Filter, renderer);
			}

			foreach (var obj in objectsDict.Values)
			{
				obj.reload(m_RenderProcessor);
			}

			Mesh mesh = new Mesh();
			mesh.name = name;

			m_Filter.sharedMesh = mesh;
		}

		public void addController(uint _ID)
		{
			if (!oldMode)
			{
				var bakedObject = m_BakedObjects.Find(obj => obj.serializedProperties.objectID == _ID);
				if (bakedObject != null)
				{
					addController(bakedObject);
				}
			}
		}

		public void removeController(uint _ID)
		{
			if (!oldMode)
			{
				var bakedObject = m_BakedObjects.Find(obj => obj.serializedProperties.objectID == _ID);
				if (bakedObject != null)
				{
					removeController(bakedObject);
				}
			}
		}

		public bool hasController(IGAFObject _Object)
		{
			return oldMode ? false : m_Controllers.Find((controller) => controller.objectID == _Object.serializedProperties.objectID) != null;
		}

		public void clear(bool _DestroyChildren)
		{
			for (int i = 0; i < m_Controllers.Count; i++)
			{
				if (Application.isEditor)
					DestroyImmediate(m_Controllers[i]);
				else
					Destroy(m_Controllers[i]);
			}

			if (_DestroyChildren)
			{
				List<GameObject> children = new List<GameObject>();
				foreach (Transform child in cachedTransform)
					children.Add(child.gameObject);

				children.ForEach((GameObject child) =>
				{
					if (Application.isPlaying)
						Destroy(child);
					else
						DestroyImmediate(child, true);
				});
			}
			else
			{
				foreach (var obj in m_Objects)
				{
					if (Application.isPlaying)
						Destroy(obj);
					else
						DestroyImmediate(obj, true);
				}
			}

			if (m_AllObjects != null)
			{
				m_AllObjects.Clear();
				m_AllObjects = null;
			}

			m_Controllers.Clear();
			m_Objects.Clear();
			m_BakedObjects.Clear();
		}

		public void updateToFrame(List<GAF.Data.GAFObjectStateData> _States, bool _Refresh)
		{
			foreach (var state in _States)
			{
				if (objectsDict.ContainsKey(state.id))
				{
					objectsDict[state.id].updateToState(state, m_RenderProcessor, _Refresh);
				}
			}

			if (!m_OldMode)
			{
				if (_Refresh)
					m_RenderProcessor.state |= GAFRenderProcessor.MeshState.VertexSet;

				m_RenderProcessor.process();
			}
		}

		#endregion // Interface

		#region MonoBehaviour

		private void OnWillRenderObject()
		{
			if (onWillRenderObject != null)
				onWillRenderObject();
		}

		#endregion // MonoBehaviour

		#region Implementation
		private void createNewModeObjects()
		{
			m_BakedObjects = new List<GAFBakedObject>();

			var objects = movieClip.asset.getObjects(movieClip.timelineID);
			var masks = movieClip.asset.getMasks(movieClip.timelineID);

			for (int i = 0; i < objects.Count; ++i)
			{
				var _objectData = objects[i];
				var _name		= getObjectName(_objectData);
				var _type		= movieClip.asset.maskedObjects.Contains((int)_objectData.id) ? ObjectType.Masked : ObjectType.Simple;

				m_BakedObjects.Add(createBakedObject(_name, _type, _objectData));
			}

			if (masks != null)
			{
				for (int i = 0; i < masks.Count; i++)
				{
					var _maskData	= masks[i];
					var _name		= getObjectName(_maskData) + "_mask";

					m_BakedObjects.Add(createBakedObject(_name, ObjectType.Mask, _maskData));
				}
			}
		}

		private void createOldModeObjects()
		{
			var objects = movieClip.asset.getObjects(movieClip.timelineID);
			var masks	= movieClip.asset.getMasks(movieClip.timelineID);
			for (int i = 0; i < objects.Count; ++i)
			{
				var _objectData = objects[i];
				var _name		= getObjectName(_objectData);
				var _type		= movieClip.asset.maskedObjects.Contains((int)_objectData.id) ? ObjectType.Masked : ObjectType.Simple;

				m_Objects.Add(createOldModeObject(_name, _type, _objectData));
			}

			if (masks != null)
			{
				for (int i = 0; i < masks.Count; ++i)
				{
					var _maskData	= masks[i];
					var _name		= getObjectName(_maskData) + "_mask";

					m_Objects.Add(createOldModeObject(_name, ObjectType.Mask, _maskData));
				}
			}
		}

		private string getObjectName(GAF.Data.GAFObjectData _Object)
		{
			var namedParts = movieClip.asset.getNamedParts(movieClip.timelineID);
			var part = namedParts.Find((partData) => partData.objectID == _Object.id);

			return part == null ? _Object.atlasElementID.ToString() + "_" + _Object.id.ToString() : part.name;
		}

		private GAFBakedObject createBakedObject(string _Name, ObjectType _Type, GAF.Data.GAFObjectData _Data)
		{
			GAFBakedObject bakedObject = new GAFBakedObject();
			bakedObject.initialize(_Name, _Type, movieClip, this, _Data.id, _Data.atlasElementID);

			return bakedObject;
		}

		private GAFObject createOldModeObject(string _Name, ObjectType _Type, GAF.Data.GAFObjectData _Data)
		{
			var gameObj = new GameObject { name = _Name };
			gameObj.transform.parent		= this.transform;
			gameObj.transform.localScale	= Vector3.one;
			gameObj.transform.localPosition	= Vector3.zero;

			var component = gameObj.AddComponent<GAFObject>();
			component.initialize(_Name, _Type, movieClip, this, _Data.id, _Data.atlasElementID);

			return component;
		}

		private void addController(GAFBakedObject _BakedObject)
		{
			var gameObj = new GameObject { name = _BakedObject.serializedProperties.name };
			gameObj.transform.parent			= this.transform;
			gameObj.transform.localScale		= Vector3.one;
			gameObj.transform.localPosition		= _BakedObject.serializedProperties.localPosition;

			var component = gameObj.AddComponent<GAFBakedObjectController>();
			_BakedObject.setController(component);
			m_Controllers.Add(component);
		}

		private void removeController(GAFBakedObject _BakedObject)
		{
			var _object = cachedTransform.FindChild(_BakedObject.serializedProperties.name);
			if (!Application.isPlaying)
				DestroyImmediate(_object.gameObject);
			else
				Destroy(_object.gameObject);

			_BakedObject.setController(null);

			m_Controllers.RemoveAll((contoller) => contoller.objectID == _BakedObject.serializedProperties.objectID);
		}

		#endregion // Implementation
	}
}