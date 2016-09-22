// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	public class LW_Styles : LW_Appearance {

		/// <summary>
		/// The collection of shapes attached to this appearance.
		/// </summary>
		public List<LW_Style> list {
			get {
				if (m_StylesList == null) {
					m_StylesList = new List<LW_Style>();
					#if UNITY_EDITOR
					m_ElementHeaderExpanded = true;
					#endif
				}
				return new List<LW_Style>(m_StylesList);
			}
			set {
				if (m_StylesList == null) {
					m_StylesList = new List<LW_Style>();
					#if UNITY_EDITOR
					m_ElementHeaderExpanded = true;
					#endif
				}
				//if (m_StylesList != value) {
				UnregisterChildren();
				m_StylesList = value;
				RegisterChildren();
				SetElementDirty();
				//}
			}
		}
		[SerializeField] protected List<LW_Style> m_StylesList;

		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Styles";
			if (m_StylesList == null) {
				m_StylesList = new List<LW_Style>();
				#if UNITY_EDITOR
				m_ElementHeaderExpanded = true;
				#endif
			}
			base.OnEnable ();
		}
		protected override void OnDestroy() {
			#if UNITY_EDITOR
			if (m_StylesList != null) for (int i=0; i<m_StylesList.Count; i++) if (m_StylesList[i] != null) LW_Utilities.SafeDestroy(m_StylesList[i]);
			#endif
			base.OnDestroy();
		}

		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Styles) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Styles>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}
		#if UNITY_EDITOR
		internal override bool SaveToAssetDatabase(Object parentAsset, bool saveAssetDatabase) {
			if (base.SaveToAssetDatabase(parentAsset, false)) {
				if (m_StylesList != null) for(int i=0; i<m_StylesList.Count; i++) if (m_StylesList[i] != null) m_StylesList[i].SaveToAssetDatabase(this, false);
				if (saveAssetDatabase) UnityEditor.AssetDatabase.SaveAssets ();
				return true;
			}
			else return false;
		}
		public override void SetHideFlags(HideFlags flag) {
			base.SetHideFlags(flag);
			if (m_StylesList != null) for(int i=0; i<m_StylesList.Count; i++) if (m_StylesList[i] != null) m_StylesList[i].SetHideFlags(flag);
		}
		#endif

		internal override void SetClean() {
			base.SetClean();
			if (m_StylesList != null) for (int i=0; i<m_StylesList.Count; i++) if (m_StylesList[i] != null) m_StylesList[i].SetClean();
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			if (element is LW_Styles) {
				LW_Styles styles = element as LW_Styles;

				bool stylesMatch = m_StylesList != null && styles.m_StylesList != null && m_StylesList.Count == styles.m_StylesList.Count;
				if (stylesMatch) {
					for (int i=0; i<m_StylesList.Count; i++) {
						if (m_StylesList[i].GetType() != styles.m_StylesList[i].GetType() || m_StylesList[i] == styles.m_StylesList[i]) {
							stylesMatch = false;
							break;
						}
					}
				}
				if (stylesMatch) {
					for (int i=0; i<m_StylesList.Count; i++) {
						m_StylesList[i].CopyPropertiesFrom(styles.m_StylesList[i]);
					}
				}
				else {
					if (m_StylesList != null) {
						for(int i=0; i<m_StylesList.Count; i++) if (m_StylesList[i] != null) LW_Utilities.SafeDestroy(m_StylesList[i]);
						m_StylesList.Clear();
					}
					if (styles.m_StylesList != null) {
						if (m_StylesList == null) m_StylesList = new List<LW_Style>(styles.m_StylesList.Count);
						else if (m_StylesList.Capacity < styles.m_StylesList.Count) m_StylesList.Capacity = styles.m_StylesList.Count;

						for(int i=0; i<styles.m_StylesList.Count; i++) {
							if (styles.m_StylesList[i] != null) {
								LW_Style copiedElement = styles.m_StylesList[i].Copy() as LW_Style;
								m_StylesList.Add(copiedElement);
							}
						}
					}
				}
			}
			base.CopyPropertiesFrom(element);
		}

		internal override void RegisterChildren() {
			base.RegisterChildren();
			if (m_StylesList != null) for (int i=0; i<m_StylesList.Count; i++) if (m_StylesList[i] != null)  m_StylesList[i].RegisterCallbacks(OnChildSetElementDirty);
		}
		internal override void UnregisterChildren() {
			base.UnregisterChildren();
			if (m_StylesList != null) for (int i=0; i<m_StylesList.Count; i++) if (m_StylesList[i] != null) m_StylesList[i].UnregisterCallbacks(OnChildSetElementDirty);
		}

		/// <summary>
		/// Indexer for the collection.
		/// </summary>
		/// <remarks>
		/// Seting an element through the indexer will mark the collection as dirty.
		/// </remarks>
		/// <param name="index"></param>
		/// <returns></returns>
		public LW_Style this[int index] {
			get {
				return m_StylesList[index];
			}
			set {
				if (m_StylesList != null && m_StylesList.Count > index) {
					if (m_StylesList[index] != value) {
						m_StylesList[index].UnregisterCallbacks(OnChildSetElementDirty);
						m_StylesList[index] = value;
						m_StylesList[index].RegisterCallbacks(OnChildSetElementDirty);
						m_StylesList[index].SetElementDirty();
						SetElementDirty();
					}
				}
				else Debug.LogError("[LineWorks Collection] Index out of range.");
			}
		}
		/// <summary>
		/// Returns the number of elements in the collection. (Read Only).
		/// </summary>
		public int Count {
			get {
				return m_StylesList != null ? m_StylesList.Count : 0;
			}
		}
		/// <summary>
		/// The current capacity of the underlying List &lt; T &gt;
		/// </summary>
		public int Capacity {
			get {
				return m_StylesList != null ? m_StylesList.Capacity : 0;
			}
			set {
				if (m_StylesList != null && m_StylesList.Capacity != value) {
					m_StylesList.Capacity = value;
				}
			}
		}

		/// <summary>
		/// Adds an element to the collection
		/// </summary>
		/// <param name="element"></param>
		public void Add(LW_Style item) {
			if (m_StylesList == null) m_StylesList = new List<LW_Style>();
			if (!m_StylesList.Contains(item)) {
				m_StylesList.Add(item);
				item.RegisterCallbacks(OnChildSetElementDirty);
				SetElementDirty();
			}
		}
		/// <summary>
		/// Adds a collection of elements.
		/// </summary>
		/// <param name="elements"></param>
		public void AddRange(LW_Style[] items) {
			for (int i = 0; i < items.Length; i++) {
				Add(items[i]);
			}
		}
		/// <summary>
		/// Adds a collection of elements.
		/// </summary>
		/// <param name="elements"></param>
		public void AddRange(List<LW_Style> items) {
			for (int i = 0; i < items.Count; i++) {
				Add(items[i]);
			}
		}
		/// <summary>
		/// Removes the provided element from the collection.
		/// </summary>
		/// <param name="element"></param>
		public bool Remove(LW_Style item) {
			if (m_StylesList != null && m_StylesList.Contains(item)) {
				item.UnregisterCallbacks(OnChildSetElementDirty);
				bool result = m_StylesList.Remove(item);
				LW_Utilities.SafeDestroy<LW_Style>(item);
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
			if (m_StylesList != null && dstIndex > -1 && dstIndex < m_StylesList.Count) {
				LW_Style item = m_StylesList[srcIndex];
				m_StylesList.RemoveAt(srcIndex);
				m_StylesList.Insert(dstIndex, item);
				SetElementDirty();
			}
		}
		/// <summary>
		/// Inserts a new element at the provided index. If an element already exists at the index. the new element is a copy of the old element.
		/// </summary>
		/// <param name="index"></param>
		public void InsertAt(int index) {
			if (m_StylesList != null && index > -1 && index < m_StylesList.Count) {
				LW_Style copiedElement = m_StylesList[index].Copy() as LW_Style;
				m_StylesList.Insert(index, copiedElement);
				m_StylesList[index].RegisterCallbacks(OnChildSetElementDirty);
				SetElementDirty();
			}
		}
		/// <summary>
		/// Removes the element at the provided index.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index) {
			if (m_StylesList != null && index > -1 && index < m_StylesList.Count) {
				Remove(m_StylesList[index]);
			}
		}
		/// <summary>
		/// Clears the underlying List &lt; T &gt;
		/// </summary>
		public void Clear() {
			if (m_StylesList != null) {
				UnregisterChildren();
				m_StylesList.Clear();
				SetElementDirty();
			}
		}
	}
}
