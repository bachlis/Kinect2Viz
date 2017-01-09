// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Base abstract class for all possible shapes.
	/// </summary>
	public abstract class LW_Graphic : LW_Element {

		/// <summary>
		/// Gets or sets the matrix.
		/// </summary>
		/// <value>The matrix.</value>
		public Matrix4x4 transform {
			get {
				//return m_Transform;
				return Matrix4x4.TRS(position, rotation, scale);
			}
			set {
				//m_Transform = value;
				position = value.ExtractTranslationFromMatrix();
				rotation = value.ExtractRotationFromMatrix();
				scale = value.ExtractScaleFromMatrix();
			}
		}
		//[SerializeField] Matrix4x4 m_Transform = Matrix4x4.identity;

		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		/// <value>The position.</value>
		public Vector3 position {
			get {
				return m_Position;
			}
			set {
				if (m_Position != value) {
					m_Position = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_Position = Vector3.zero;

		/// <summary>
		/// Gets or sets the rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public Quaternion rotation {
			get {
				return Quaternion.Euler(m_EulerRotation);
			}
			set {
				eulerRotation = value.eulerAngles;
			}
		}

		/// <summary>
		/// Gets or sets the euler rotation.
		/// </summary>
		/// <value>The euler rotation.</value>
		public Vector3 eulerRotation {
			get {
				return m_EulerRotation;
			}
			set {
				if (m_EulerRotation != value) {
					m_EulerRotation = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_EulerRotation = Vector3.zero;

		/// <summary>
		/// Gets or sets the scale.
		/// </summary>
		/// <value>The scale.</value>
		public Vector3 scale {
			get {
				return m_Scale;
			}
			set {
				if (m_Scale != value) {
					m_Scale = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_Scale = Vector3.one;

		/// <summary>
		/// Gets or sets the styles colleciton.
		/// </summary>
		/// <value>The LW_Styles value is the styles collection element.</value>
		public LW_Styles styles {
			get {
				//if (m_Styles == null) {
				//	m_Styles = CreateInstance<LW_Styles>();
					//m_Styles.SetHideFlags(hideFlags);
				//}
				return m_Styles;
			}
			set {
				if (m_Styles != value) {
					UnregisterChildren();
					if (m_Styles != null) m_Styles = LW_Utilities.SafeDestroy<LW_Styles>(m_Styles);
					m_Styles = value;
					RegisterChildren();
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected LW_Styles m_Styles;

		/// <summary>
		/// Set the specified matrix and styles.
		/// </summary>
		/// <param name="matrix">Matrix.</param>
		/// <param name="styles">styles.</param>
		public void Set(Matrix4x4 matrix = default(Matrix4x4), LW_Styles styles = null) {
			if (matrix == Matrix4x4.zero) matrix = Matrix4x4.identity;
			this.transform = matrix;
			this.styles = styles;
		}
		public void Set(Vector3 position, Vector3 eulerRotation, Vector3 scale) {
			this.position = position;
			this.eulerRotation = eulerRotation;
			this.scale = scale;
		}
		public void Set(Vector3 position, Quaternion rotation, Vector3 scale) {
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
		}
			
		protected override void OnDestroy() {
			#if UNITY_EDITOR
			if (m_Styles != null) m_Styles = LW_Utilities.SafeDestroy<LW_Styles>(m_Styles);
			#endif
			base.OnDestroy();
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Graphic";
			if (m_Styles == null) {
				m_Styles = CreateInstance<LW_Styles>();
				#if UNITY_EDITOR
				m_Styles.SetHideFlags(hideFlags);
				#endif
			}

			base.OnEnable();
		}

		public override void SetElementDirty() {
			base.SetElementDirty();
			RebuildShape();
		}
			
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Graphic) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Graphic>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		#if UNITY_EDITOR
		internal override bool SaveToAssetDatabase(Object parentAsset, bool saveAssetDatabase) {
			if (base.SaveToAssetDatabase(parentAsset, false)) {
				if (m_Styles != null) m_Styles.SaveToAssetDatabase(this, false);
				if (saveAssetDatabase) UnityEditor.AssetDatabase.SaveAssets ();
				return true;
			}
			else return false;
		}
		public override void SetHideFlags(HideFlags flag) {
			base.SetHideFlags(flag);
			if (m_Styles != null) m_Styles.SetHideFlags(flag);
		}
		#endif
			
		internal override void CopyPropertiesFrom(LW_Element element) {
			if (element is LW_Graphic) {
				LW_Graphic graphic = element as LW_Graphic;
				m_Position = graphic.m_Position;
				m_EulerRotation = graphic.m_EulerRotation;
				m_Scale = graphic.m_Scale;
				if (m_Styles != null) m_Styles.CopyPropertiesFrom(graphic.m_Styles);
				RebuildShape();
			}
			base.CopyPropertiesFrom(element);
		}
		internal override void SetClean() {
			base.SetClean();
			if (m_Styles != null) m_Styles.SetClean();
		}

		internal override void RegisterChildren() {
			base.RegisterChildren();
			if (m_Styles != null) m_Styles.RegisterCallbacks(OnChildSetElementDirty);
		}
		internal override void UnregisterChildren() {
			base.UnregisterChildren();
			if (m_Styles != null) m_Styles.UnregisterCallbacks(OnChildSetElementDirty);
		}
			
		internal virtual void RebuildShape(bool force = false) {
			if (m_ElementDirty || force) {
			}
		}
	}
}