// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines a line.
	/// </summary>
	public class LW_Line : LW_Vector2Shape {

		/// <summary>
		/// Start point.
		/// </summary>
		public Vector2 start {
			get {
				return m_Start;
			}
			set {
				if (m_Start != value) {
					m_Start = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector2 m_Start = new Vector2(-40,40);

		/// <summary>
		/// End point.
		/// </summary>
		public Vector2 end {
			get {
				return m_End;
			}
			set {
				if (m_End != value) {
					m_End = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector2 m_End = new Vector2(40,-40);

		/// <summary>
		/// Create the specified start and end.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		public static LW_Line Create(Vector2 start = default(Vector2), Vector2 end = default(Vector2)) {
			LW_Line instance = CreateInstance<LW_Line>();
			instance.Set(start, end);
			return instance;
		}

		/// <summary>
		/// Set all properties of the line in one call.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public void Set(Vector2 start = default(Vector2), Vector2 end = default(Vector2)) {
			m_IsClosed = false;
			if (start == Vector2.zero && end == Vector2.zero) {
				m_Start = new Vector2(-40,40);
				m_End = new Vector2(40,-40);
			}
			else {
				m_Start = start;
				m_End = end;
			}
			SetElementDirty();
		}

		/// <summary>
		/// Creates a copy of this line.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Line) {
				element.CopyPropertiesFrom (this);
			}
			else {
				element = Copy(CreateInstance<LW_Line>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Line line = element as LW_Line;
			if (line != null) {
				m_Start = line.m_Start;
				m_End = line.m_End;
			}
			base.CopyPropertiesFrom(element);
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Line";
			base.OnEnable();
		}
		internal override void RebuildShape(bool force = false) {
			if (m_ElementDirty || force) {
				m_IsClosed = false;
				if (m_Points == null) new List<Vector3>(2);
				else m_Points.Clear();

				m_Points.Add(m_Start);
				m_Points.Add(m_End);
			}
			base.RebuildShape(force);
		}
	}
}