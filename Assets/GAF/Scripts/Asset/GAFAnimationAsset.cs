/*
 * File:			gafanimationasset.cs
 * Version:			3.10
 * Last changed:	2014/10/20 17:40
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using System.IO;

using GAF.Utils;
using GAF.Data;
using GAF.Reader;

namespace GAF.Assets
{
	[System.Serializable]
	public class GAFAnimationAsset : ScriptableObject
	{
		#region Members

		[HideInInspector][SerializeField]private int		m_AssetVersion	= 0;
		[HideInInspector][SerializeField]private string		m_GUID			= string.Empty;
		[HideInInspector][SerializeField]private byte[]		m_AssetData		= null;

		[HideInInspector][SerializeField]private bool		m_IsSharedDataCollected	= false;
		[HideInInspector][SerializeField]private List<int>	m_BatchedObjects		= new List<int>();
		[HideInInspector][SerializeField]private List<int>	m_MaskedObjects			= new List<int>();

		private Dictionary<KeyValuePair<float, float>, GAFTexturesResource> m_LoadedResources = new Dictionary<KeyValuePair<float, float>, GAFTexturesResource>();

		private GAFAnimationData m_SharedData = null;

		private Object m_Locker = new Object();

		#endregion // Members

		#region Interface

		public void initialize(byte[] _GAFData, string _GUID)
		{
			m_AssetData		= _GAFData;
			m_SharedData	= null;
			m_GUID			= _GUID;

			m_AssetVersion = GAFSystem.AssetVersion;

			load();
		}

		public void resetGUID(string _GUID)
		{
			m_GUID = _GUID;
		}

		public void load()
		{
			lock (m_Locker)
			{
#if UNITY_EDITOR
				if (m_AssetVersion < GAFSystem.AssetVersion &&
					!EditorApplication.isPlayingOrWillChangePlaymode)
				{
					upgrade();
				}
#endif // UNITY_EDITOR

				if (!isLoaded &&
					 m_AssetVersion == GAFSystem.AssetVersion)
				{
					if (m_AssetData != null)
					{
						GAFReader reader = new GAFReader();
						try
						{
							reader.Load(m_AssetData, ref m_SharedData);
						}
						catch (GAFException _Exception)
						{
							GAFUtils.Error(_Exception.Message);

							m_SharedData = null;
						}

						if (isLoaded &&
							!m_IsSharedDataCollected)
						{
							collectSharedData();

#if UNITY_EDITOR
							if (!EditorApplication.isPlayingOrWillChangePlaymode)
								EditorUtility.SetDirty(this);
#endif // UNITY_EDITOR
						}
					}
				}
			}
		}

		public GAFTexturesResource getResource(float _Scale, float _CSF)
		{
			GAFTexturesResource resource = null;

			var key = new KeyValuePair<float, float>(_Scale, _CSF);
			if (m_LoadedResources.ContainsKey(key) &&
				m_LoadedResources[key] != null)
			{
				resource = m_LoadedResources[key];
			}
			else
			{
				string resourcePath = "Cache/" + getResourceName(_Scale, _CSF);
				resource = Resources.Load<GAFTexturesResource>(resourcePath);
				if (resource != null)
					m_LoadedResources[key] = resource;
			}

			return resource;
		}

		public string getResourceName(float _Scale, float _CSF)
		{
			return "[" + name + "]" + m_GUID + "_" + _Scale.ToString() + "_" + _CSF.ToString();
		}

		public List<GAFTimelineData> getTimelines()
		{
			return isLoaded ? m_SharedData.timelines.Values.ToList() : null;
		}

		public List<GAFAtlasData> getAtlases(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].atlases;
			}
			else
			{
				return null;
			}
		}

		public List<GAFObjectData> getObjects(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].objects;
			}
			else
			{
				return null;
			}
		}

		public List<GAFObjectData> getMasks(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].masks;
			}
			else
			{
				return null;
			}
		}

		public Dictionary<uint, GAFFrameData> getFrames(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].frames;
			}
			else
			{
				return null;
			}
		}

		public List<GAFSequenceData> getSequences(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].sequences;
			}
			else
			{
				return null;
			}
		}

		public List<string> getSequenceIDs(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].sequences.ConvertAll(sequence => sequence.name);
			}
			else
			{
				return null;
			}
		}

		public List<GAFNamedPartData> getNamedParts(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].namedParts;
			}
			else
			{
				return null;
			}
		}

		public uint getFramesCount(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].framesCount;
			}
			else
			{
				return (uint)0;
			}
		}

		public Rect getFrameSize(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].frameSize;
			}
			else
			{
				return new Rect(0, 0, 0, 0);
			}
		}

		public Vector2 getPivot(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].pivot;
			}
			else
			{
				return Vector2.zero;
			}
		}

		#endregion // Interface

		#region Properties

		public bool isLoaded
		{
			get
			{
				return m_SharedData != null;
			}
		}

		public bool isResourcesAvailable
		{
			get
			{
				string assetsPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
				foreach (var path in resourcesPaths)
				{
					if (!System.IO.File.Exists(assetsPath + path))
					{
						return false;
					}
				}

				return true;
			}
		}

		public int assetVersion
		{
			get
			{
				return m_AssetVersion;
			}
		}

		public ushort majorDataVersion
		{
			get
			{
				return isLoaded ? m_SharedData.majorVersion : (ushort)0;
			}
		}

		public ushort minorDataVersion
		{
			get
			{
				return isLoaded ? m_SharedData.minorVersion : (ushort)0;
			}
		}

		public List<string> resourcesPaths
		{
			get
			{
				List<string> paths = new List<string>();
				foreach (var scale in scales)
				{
					foreach (var csf in csfs)
					{
						var resourceName = getResourceName(scale, csf) + ".asset";
						paths.Add(GAFSystem.getCachePath() + resourceName);
					}
				}
				return paths;
			}
		}

		public List<float> scales
		{
			get
			{
				return isLoaded ? m_SharedData.scales : null;
			}
		}

		public List<float> csfs
		{
			get
			{
				return isLoaded ? m_SharedData.csfs : null;
			}
		}

		public bool hasMasks
		{
			get
			{
				return maskedObjects.Count > 0;
			}
		}

		public List<int> batchableObjects
		{
			get
			{
				return m_BatchedObjects;
			}
		}

		public List<int> maskedObjects
		{
			get
			{
				return m_MaskedObjects;
			}
		}

		#endregion // Properties

		#region ScriptableObject

		private void OnEnable()
		{
			load();
		}

		#endregion // ScriptableObject

		#region Implementation

#if UNITY_EDITOR
		private void upgrade()
		{
			m_AssetVersion = GAFSystem.AssetVersion;

			m_IsSharedDataCollected = false;

			EditorUtility.SetDirty(this);
		}
#endif // UNITY_EDITOR

		private void collectSharedData()
		{
			if (isLoaded &&
				!m_IsSharedDataCollected)
			{
				m_IsSharedDataCollected = true;
				m_BatchedObjects = new List<int>();
				m_MaskedObjects = new List<int>();

				foreach (var timeline in m_SharedData.timelines.Values)
				{
					foreach (var obj in timeline.objects)
					{
						int type = 0;
						foreach (var frame in timeline.frames.Values)
						{
							if (frame.states.ContainsKey(obj.id))
							{
								var state = frame.states[obj.id];

								if (state.maskID > 0)
								{
									type |= 1;
								}
							}
						}

						int id = (int)obj.id;
						switch (type)
						{
							case 0: m_BatchedObjects.Add(id); break;
							case 1: m_MaskedObjects.Add(id); break;
						}
					}
				}

				if (m_BatchedObjects.Count > 0)
				{
					GAFUtils.Log("Your animation asset has batchable objects. Make sure that dynamic batching is enabled for you project.", "");
				}
			}
		}

		#endregion // Implementation
	}
}
