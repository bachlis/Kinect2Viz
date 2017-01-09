// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	public enum ColliderType { Polygon, Edge }
	/// <summary>
	/// A Collider style that creates a 2D collider from the associated shapes.
	/// </summary>
	public class LW_Collider : LW_Style {

		/// <summary>
		/// the type of 2D collider to create.
		/// </summary>
		/// <value>
		/// -	`Polygon` - Adds a polygon collider.
		/// -	`Edge` - Adds an edge collider.
		/// </value>
		public ColliderType colliderType {
			get {
				//if ((int)(m_ColliderType.Value) == -1) m_ColliderType.Value = ColliderType.Polygon;
				return m_ColliderType;
			}
			set {
				if (m_ColliderType != value) {
					m_ColliderType = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected ColliderType m_ColliderType = ColliderType.Polygon;

		/// <summary>
		/// Set all properties of the collider in one call.
		/// </summary>
		/// <param name="colliderType"></param>
		public void Set(ColliderType colliderType = ColliderType.Polygon) {
			this.colliderType = colliderType;
			SetElementDirty();
		}

		public static LW_Collider Create(ColliderType colliderType = ColliderType.Polygon) {
			LW_Collider instance = CreateInstance<LW_Collider>();
			instance.Set(colliderType);
			return instance;
		}

		/// <summary>
		/// Copy collider.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Collider) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Collider>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}
			
		internal override void Reset() {
			base.Reset();
			m_ColliderType = ColliderType.Polygon;
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Collider collider = element as LW_Collider;
			if (collider != null) {
				m_ColliderType = collider.m_ColliderType;
			}
			base.CopyPropertiesFrom(element);
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Collider";
			base.OnEnable();
		}
	}
}
