// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines a Group.
	/// </summary>
	/// <remarks>
	/// This is a container class for nesting/grouping shapes into a heirarchical tree.
	/// </remarks>
	public class LW_Group: LW_Graphic {

		/// <summary>
		/// The collection of children graphics parented to this graphic.
		/// </summary>
		/// <remarks>
		/// This creates an internal nested hierarchy of graphics.
		/// </remarks>
		public List<LW_Graphic> list {
			get {
				if (m_GraphicsList == null) m_GraphicsList = new List<LW_Graphic>();
				return new List<LW_Graphic>(m_GraphicsList);
			}
			set {
				if (m_GraphicsList == null) m_GraphicsList = new List<LW_Graphic>();
				//if (m_GraphicsList != value) {
				UnregisterChildren();
				m_GraphicsList = value;
				RegisterChildren();
				SetElementDirty();
				//}
			}
		}
		[SerializeField] protected List<LW_Graphic> m_GraphicsList;

		/// <summary>
		/// Create the specified graphicsList.
		/// </summary>
		/// <param name="graphicsList">Graphics list.</param>
		public static LW_Group Create(List<LW_Graphic> graphicsList = null) {
			LW_Group instance = CreateInstance<LW_Group>();
			instance.Set(graphicsList);
			return instance;
		}

		/// <summary>
		/// Set the specified graphicsList.
		/// </summary>
		/// <param name="graphicsList">Graphics list.</param>
		public void Set(List<LW_Graphic> graphicsList = null) {
			list = graphicsList;
		}

		protected override void OnDestroy() {
			#if UNITY_EDITOR
			if (m_GraphicsList != null) for (int i=0; i<m_GraphicsList.Count; i++) if (m_GraphicsList[i] != null) LW_Utilities.SafeDestroy(m_GraphicsList[i]);
			m_GraphicsList = null;
			#endif
			base.OnDestroy();
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Group";
			if (m_GraphicsList == null) m_GraphicsList = new List<LW_Graphic>();
			base.OnEnable();
		}

		/// <summary>
		/// Creates a copy of this group.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Group) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Group>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		#if UNITY_EDITOR
		internal override bool SaveToAssetDatabase(Object parentAsset, bool saveAssetDatabase) {
			if (base.SaveToAssetDatabase(parentAsset, false)) {
				if (m_GraphicsList != null) for(int i=0; i<m_GraphicsList.Count; i++) if (m_GraphicsList[i] != null) m_GraphicsList[i].SaveToAssetDatabase(this, false);
				if (saveAssetDatabase) UnityEditor.AssetDatabase.SaveAssets ();
				return true;
			}
			else return false;
		}
		public override void SetHideFlags(HideFlags flag) {
			base.SetHideFlags(flag);
			if (m_GraphicsList != null) for(int i=0; i<m_GraphicsList.Count; i++) if (m_GraphicsList[i] != null) m_GraphicsList[i].SetHideFlags(flag);
		}
		#endif

		internal override void SetClean() {
			base.SetClean();
			if (m_GraphicsList != null) for (int i=0; i<m_GraphicsList.Count; i++) if (m_GraphicsList[i] != null) m_GraphicsList[i].SetClean();
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			if (element is LW_Group) {
				LW_Group group = element as LW_Group;

				bool groupMatches = m_GraphicsList != null && group.m_GraphicsList != null && m_GraphicsList.Count == group.m_GraphicsList.Count;
				if (groupMatches) {
					for (int i=0; i<m_GraphicsList.Count; i++) {
						if (m_GraphicsList[i].GetType() != group.m_GraphicsList[i].GetType() || m_GraphicsList[i] == group.m_GraphicsList[i]) {
							groupMatches = false;
							break;
						}
					}
				}
				if (groupMatches) {
					for (int i=0; i<m_GraphicsList.Count; i++) {
						m_GraphicsList[i].CopyPropertiesFrom(group.m_GraphicsList[i]);
					}
				}
				else {
					if (m_GraphicsList != null) {
						for(int i=0; i<m_GraphicsList.Count; i++) if (m_GraphicsList[i] != null) LW_Utilities.SafeDestroy(m_GraphicsList[i]);
						m_GraphicsList = null;
					}
					if (group.m_GraphicsList != null) {
						if (m_GraphicsList == null) m_GraphicsList = new List<LW_Graphic>(group.m_GraphicsList.Count);
						else if (m_GraphicsList.Capacity < group.m_GraphicsList.Count) m_GraphicsList.Capacity = group.m_GraphicsList.Count;

						for(int i=0; i<group.m_GraphicsList.Count; i++) {
							if (group.m_GraphicsList[i] != null) {
								LW_Graphic copiedElement = group.m_GraphicsList[i].Copy() as LW_Graphic;
								m_GraphicsList.Add(copiedElement);
							}
						}
					}
				}
			}
			base.CopyPropertiesFrom(element);
		}

		internal override void RegisterChildren() {
			base.RegisterChildren();
			if (m_GraphicsList != null) for (int i=0; i<m_GraphicsList.Count; i++) if (m_GraphicsList[i] != null) m_GraphicsList[i].RegisterCallbacks(OnChildSetElementDirty);
		}
		internal override void UnregisterChildren() {
			base.UnregisterChildren();
			if (m_GraphicsList != null) for (int i=0; i<m_GraphicsList.Count; i++) if (m_GraphicsList[i] != null) m_GraphicsList[i].UnregisterCallbacks(OnChildSetElementDirty);
		}

		internal override void RebuildShape(bool force = false) {
			if (m_ElementDirty || force) {
				if (m_GraphicsList != null) for (int i=0; i<m_GraphicsList.Count; i++) if (m_GraphicsList[i] != null) m_GraphicsList[i].RebuildShape(force);
			}
			base.RebuildShape(force);
		}

		/// <summary>
		/// Indexer for the collection.
		/// </summary>
		/// <remarks>
		/// Seting an element through the indexer will mark the collection as dirty.
		/// </remarks>
		/// <param name="index"></param>
		/// <returns></returns>
		public LW_Graphic this[int index] {
			get {
				return m_GraphicsList[index];
			}
			set {
				if (m_GraphicsList != null && m_GraphicsList.Count > index) {
					if (m_GraphicsList[index] != value) {
						m_GraphicsList[index].UnregisterCallbacks(OnChildSetElementDirty);
						m_GraphicsList[index] = value;
						m_GraphicsList[index].RegisterCallbacks(OnChildSetElementDirty);
						m_GraphicsList[index].SetElementDirty();
						SetElementDirty();
					}
				}
				else Debug.LogError("[LineWorks Collection] Index out of range.");
			}
		}
		/// <summary>
		/// Returns the number of graphics in the collection. (Read Only).
		/// </summary>
		public int Count {
			get {
				return m_GraphicsList != null ? m_GraphicsList.Count : 0;
			}
		}
		/// <summary>
		/// The current capacity of the underlying List &lt; T &gt;
		/// </summary>
		public int Capacity {
			get {
				return m_GraphicsList != null ? m_GraphicsList.Capacity : 0;
			}
			set {
				if (m_GraphicsList != null && m_GraphicsList.Capacity != value) {
					m_GraphicsList.Capacity = value;
				}
			}
		}

		/// <summary>
		/// Adds an element to the collection
		/// </summary>
		/// <param name="element"></param>
		public void Add(LW_Graphic item) {
			if (m_GraphicsList == null) m_GraphicsList = new List<LW_Graphic>();
			if (!m_GraphicsList.Contains(item)) {
				m_GraphicsList.Add(item);
				item.RegisterCallbacks(OnChildSetElementDirty);
				SetElementDirty();
			}
		}
		/// <summary>
		/// Adds a collection of graphics.
		/// </summary>
		/// <param name="graphics"></param>
		public void AddRange(LW_Graphic[] items) {
			for (int i = 0; i < items.Length; i++) {
				Add(items[i]);
			}
		}
		/// <summary>
		/// Adds a collection of graphics.
		/// </summary>
		/// <param name="graphics"></param>
		public void AddRange(List<LW_Graphic> items) {
			for (int i = 0; i < items.Count; i++) {
				Add(items[i]);
			}
		}
		/// <summary>
		/// Removes the provided element from the collection.
		/// </summary>
		/// <param name="element"></param>
		public bool Remove(LW_Graphic item) {
			if (m_GraphicsList != null && m_GraphicsList.Contains(item)) {
				item.UnregisterCallbacks(OnChildSetElementDirty);
				bool result = m_GraphicsList.Remove(item);
				LW_Utilities.SafeDestroy<LW_Graphic>(item);
				SetElementDirty();
				return result;
			}
			else return false;
		}
		/// <summary>
		/// Moves an element from the srcIndex to the dstIndex
		/// </summary>
		/// <param name="srcIndex"></param>
		/// <param name="dstIndex"></param>
		public void Move(int srcIndex, int dstIndex) {
			if (m_GraphicsList != null && dstIndex > -1 && dstIndex < m_GraphicsList.Count) {
				LW_Graphic item = m_GraphicsList[srcIndex];
				m_GraphicsList.RemoveAt(srcIndex);
				m_GraphicsList.Insert(dstIndex, item);
				SetElementDirty();
			}
		}
		/// <summary>
		/// Inserts a new element at the provided index. If an element already exists at the index. the new element is a copy of the old element.
		/// </summary>
		/// <param name="index"></param>
		public void InsertAt(int index) {
			if (m_GraphicsList != null && index > -1 && index < m_GraphicsList.Count) {
				LW_Graphic copiedElement = m_GraphicsList[index].Copy() as LW_Graphic;
				m_GraphicsList.Insert(index, copiedElement);
				m_GraphicsList[index].RegisterCallbacks(OnChildSetElementDirty);
				SetElementDirty();
			}
		}
		/// <summary>
		/// Removes the element at the provided index.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index) {
			if (m_GraphicsList != null && index > -1 && index < m_GraphicsList.Count) {
				Remove(m_GraphicsList[index]);
			}
		}
		/// <summary>
		/// Clears the underlying List &lt; T &gt;
		/// </summary>
		public void Clear() {
			if (m_GraphicsList != null) {
				UnregisterChildren();
				m_GraphicsList.Clear();
				SetElementDirty();
			}
		}
	}
}
