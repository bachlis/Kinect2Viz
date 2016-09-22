// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LineWorks {

	/// <summary>
	/// This is the base abstract class for all LineWorks Elements. 
	/// </summary>
	/// <remarks>
	/// As derived from Scriptable Object, it is a reference type and can be saved as an asset in project by selecting "Save To Assets" in the elements inspector dropdown menu.
	/// </remarks>
	public abstract class LW_Element : ScriptableObject {


		protected static bool s_Debug = false;

		internal int id {
			get {
				if (m_Id == -1) m_Id = GetHashCode();
				return m_Id;
			}
			set {
				if (m_Id != value) {
					m_Id = value;
				}
			}
		}
		[SerializeField] protected int m_Id = -1;

		/// <summary>
		/// Gets or sets the elements name.
		/// </summary>
		/// <value>The string value represents the element's name.</value>
		public new string name {
			get {
				return base.name;
			}
			set {
				base.name = value;
			}
		}
			
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="LineWorks.LW_Element"/> is visible.
		/// </summary>
		/// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
		public virtual bool isVisible {
			get {
				return m_IsVisible;
			}
			set {
				if (m_IsVisible != value) {
					m_IsVisible = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_IsVisible  = true;


		/// <summary>
		/// Returns if an element is not registered with any parents. (Read-only).
		/// </summary>
		public bool isOrphaned{
			get {
				if (m_OnElementDirtyCallback == null) {
					return true;
				}
				else {
					System.Delegate[] delegates = m_OnElementDirtyCallback.GetInvocationList();
					return delegates == null || delegates.Length <= 0;
				}
			}
		}

		/// <summary>
		/// Returns if an element is currently marked as dirty. (Read-only).
		/// </summary>
		public bool isDirty {
			get {
				return m_ElementDirty;
			}
		}

		protected bool m_IsEnabled = false;
		protected bool m_ElementDirty = true;
		protected UnityAction m_OnElementDirtyCallback;

		protected virtual void OnEnable() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("OnEnable LW_Element: " + name);
			#endif

			/* This was for dealing with gameObject duplication. but it needs some work because it creates a copy everytime the scene is loaded.
			#if UNITY_EDITOR
			int instanceID = GetHashCode();
			if (m_Id == -1) m_Id = instanceID;
			else if (m_Id != instanceID) {
				if (s_Debug) Debug.Log("LW_Element InstanceId Out-of-Date: " + name);
				m_Id = instanceID;
			}
			#endif
			*/

			//SetClean();
			//SetElementDirty();
			UnregisterChildren();
			RegisterChildren();
			m_IsEnabled = true;
		}
		protected virtual void OnDisable() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("OnDisable: " + name);
			#endif

			UnregisterChildren();
			m_IsEnabled = false;
		}
		protected virtual void OnDestroy() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("OnDestroy: " + name);
			#endif

			UnregisterChildren();
			m_IsEnabled = false;
		}

		#if UNITY_EDITOR
		protected virtual void OnValidate() {
			if (s_Debug) Debug.Log("OnValidate: " + name + " isEnabled: " + m_IsEnabled);

			if (m_IsEnabled) {
				SetElementDirty();
			}
		}
		[System.NonSerialized] public LW_Graphic m_SelectedGraphic = null;
		[System.NonSerialized] public LW_Appearance m_SelectedAppearance = null;
		public bool m_ElementPropertiesExpanded = false;
		public bool m_ElementHeaderExpanded = false;

		/// <summary>
		/// Saves Element to AssetDatabase. Only available in Editor.
		/// </summary>
		/// <returns><c>true</c>, if to asset database was saved, <c>false</c> otherwise.</returns>
		/// <param name="parentAsset">Parent asset.</param>
		/// <param name="saveAssetDatabase">If set to <c>true</c> AssetDatabase will be updated. This is for internal use only. Always leave blank or <c>true</c>.</param>
		public virtual bool SaveToAssetDatabase(Object parentAsset = null) {
			return SaveToAssetDatabase(parentAsset, true);
		}
		internal virtual bool SaveToAssetDatabase(Object parentAsset, bool saveAssetDatabase) {
			if (s_Debug) Debug.Log("SaveToAssetDatabase: " + this.name + (parentAsset == null ? "" : " to: " + parentAsset.name) );

			if (!Application.isPlaying && !AssetDatabase.Contains(this)) {
				if (parentAsset == null) {
					if (s_Debug) Debug.Log("No parentAsset is provided. Creating a new main asset.");
					string filePath = AssetDatabase.GetAssetPath(Selection.activeObject);
					string fileName = "/" + name + ".asset";
					if (string.IsNullOrEmpty(filePath)) filePath = "Assets";
					else if (System.IO.Path.GetExtension(filePath) != "") filePath = filePath.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
					string assetPath = AssetDatabase.GenerateUniqueAssetPath(filePath + fileName);
					this.hideFlags = HideFlags.None;
					AssetDatabase.CreateAsset(this, assetPath);
				}
				else if (!AssetDatabase.Contains(parentAsset)) {
					if (s_Debug) Debug.Log("A parentAsset is provided but didn't save the parent first.  This condition should not occur.");
					string filePath = AssetDatabase.GetAssetPath(Selection.activeObject);
					string fileName = "/" + parentAsset.name + ".asset";
					if (string.IsNullOrEmpty(filePath)) filePath = "Assets";
					else if (System.IO.Path.GetExtension(filePath) != "") filePath = filePath.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
					string assetPath = AssetDatabase.GenerateUniqueAssetPath(filePath + fileName);
					parentAsset.hideFlags = HideFlags.None;
					AssetDatabase.CreateAsset(parentAsset, assetPath);
					this.hideFlags = s_Debug || parentAsset is GameObject ? HideFlags.None : HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset(this, parentAsset);
				}
				else {
					if (s_Debug) Debug.Log("A parentAsset is provided and it's already in the AssetDatabase, then we add this as a subAsset of the parent.");
					this.hideFlags = s_Debug || parentAsset is GameObject ? HideFlags.None : HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset(this, parentAsset);
				}
				if (saveAssetDatabase) AssetDatabase.SaveAssets();
				return true;
			}
			else {
				Debug.Log("Cannot save asset. " + this.name + " is already saved to the AssetDatabase.");
				return false;
			}
		}

		/// <summary>
		/// Sets the hide flags. This an Editor-Only helper method to set the HideFlags.
		/// </summary>
		/// <param name="flag">Flag.</param>
		public virtual void SetHideFlags(HideFlags flag) {
			if (Application.isPlaying) return;
			if (s_Debug) Debug.Log("SetHideFlags: " + name + " flag: " + flag);
			if (this != null) this.hideFlags = flag;
		}
		#endif

		/// <summary>
		/// Sets an element dirty. The Element will be rebuilt and cleaned on the next Canvas update cycle.
		/// </summary>
		/// <remarks>
		/// All LW_Elements use the Unity UI CanvasUpdateRegistry for rebuilding events. 
		/// This means that Rebuilds will occur when the CanvasUpdateRegistry feels it necessary. Typically this will occur on the next frame.
		/// </remarks>
		public virtual void SetElementDirty() {
			if (m_ElementDirty) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("SetElementDirty: " + name);
			#endif

			#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
			SceneView.RepaintAll();
			#endif

			UnregisterChildren();
			RegisterChildren();
			m_ElementDirty = true;
			if (m_OnElementDirtyCallback != null) m_OnElementDirtyCallback();
		}

		/// <summary>
		/// Creates a copy of this element or Copies the properties of this element on to a provided element.
		/// </summary>
		/// <param name="element">The element to apply this elements properties on to. if null a new element is created.</param>
		/// <returns></returns>
		public virtual LW_Element Copy(LW_Element element = null) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("Copy: " + name);
			#endif

			if (element != null) element.CopyPropertiesFrom(this);
			return element;
		}

		internal virtual void OnChildSetElementDirty() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("OnChildSetElementDirty: " + name);
			#endif

			m_ElementDirty = true;
			if (m_OnElementDirtyCallback != null) m_OnElementDirtyCallback();
		}
		internal virtual void SetClean() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("SetElementSetClean: " + name);
			#endif

			m_ElementDirty = false;
		}
		internal virtual void CopyPropertiesFrom(LW_Element element) {
			if (element == null || this == null) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("CopyElementPropertiesFrom: " + element.name + " to: " + name);
			#endif

			hideFlags = element.hideFlags;
			name = element.name;
			//name = element.name + " (Copy)";

			//m_Id = element.m_Id;
			//m_Id = GetHashCode();
			m_IsVisible = element.m_IsVisible;

			// NonSerialized
			m_ElementDirty = element.m_ElementDirty;
			m_OnElementDirtyCallback = element.m_OnElementDirtyCallback;

			UnregisterChildren();
			RegisterChildren();
		}
		internal virtual void RegisterCallbacks(UnityAction elementDirtyAction = null) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("RegisterCallbacks LW_Element: " + name + (elementDirtyAction != null ? " with: " + (elementDirtyAction.Target as UnityEngine.Object).name : ""));
			#endif

			if (elementDirtyAction != null) m_OnElementDirtyCallback += elementDirtyAction;
			//RegisterChildren();
		}
		internal virtual void UnregisterCallbacks(UnityAction elementDirtyAction = null) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("UnregisterCallbacks LW_Element: " + name + (elementDirtyAction != null ? " with: " + (elementDirtyAction.Target as UnityEngine.Object).name : ""));
			#endif

			if (elementDirtyAction != null) m_OnElementDirtyCallback -= elementDirtyAction;
			//UnregisterChildren();
		}
		internal virtual void UnregisterCallbacks() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("UnregisterCallbacks LW_Element: " + name);
			#endif

			m_OnElementDirtyCallback = null;
			//UnregisterChildren();
		}
		internal virtual void RegisterChildren() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("RegisterChildren LW_Element: " + name);
			#endif
		}
		internal virtual void UnregisterChildren() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_Debug) Debug.Log("UnregisterChildren LW_Element: " + name);
			#endif
		}
	}
}
