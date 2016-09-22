// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

    /// <summary>
    /// Internal class used for simplifying a collection of ContourVertexs.
    /// </summary>
    /// <remarks>
    /// The simplification Algorithm uses a method known as the Visvalingam and Whyatt (VW) algorithm. Basically it takes a collection of points, calculates the area of the triangle each point makes with its neighbor points and then recursively removes points whose triangle area is below the simplification value.
    /// </remarks>
	internal static class LW_Simplifier {
		private static List<Vector3> s_Points;
		private static Vector3[] cPoints;
		private static List<int> s_content;
		private static HashSet<int> s_removed;
		private static int s_NumberOfItems;
		private static bool s_PreserveAcute = true;

		internal static void SimplifyPoints(ref List<Vector3> points, float areaThreshold, bool preserveAcuteTriangles = true) {
			if (s_Points == null) s_Points = new List<Vector3>(points.Count);
			else {
				s_Points.Clear();
				if (s_Points.Capacity < points.Count) s_Points.Capacity = points.Count;
			}
			for (int i=0; i<points.Count; i++) s_Points.Add(points[i]);

			if (s_content == null) s_content = new List<int>(points.Count);
			else {
				s_content.Clear();
				if (s_content.Capacity < points.Count) s_content.Capacity = points.Count;
			}

			if (s_removed == null) s_removed = new HashSet<int>();
			else s_removed.Clear();

			s_NumberOfItems = 0;
			s_PreserveAcute = preserveAcuteTriangles;

			for (int i=0; i<s_Points.Count; i++) insert(i);

			//DumpPoints
			bool belowThreshold = true;
			while (belowThreshold) {
				int pointIndex = getMin();
				if (triangleArea(pointIndex) <= areaThreshold && pointIndex != 0 && pointIndex != s_Points.Count - 1) deleteMin();
				else belowThreshold = false;
			}

			points.Clear();
			for (int i = 0; i < s_Points.Count; i++) {
				if (!s_removed.Contains(i)) points.Add(s_Points[i]);
			}
		}

		private static bool isEmpty() {
			if (getCount() == 0) return true;
			else return false;
		}
		private static int getCount() {
			return s_NumberOfItems;
		}

		private static int getParentIndex(int index) {
			if (index < 0) return 0;
			if (index > getCount()-1) return getCount()-1;
			int result = (int)Mathf.Floor(((float)index - 1) / 2f);
			return result;
		}
		private static int getLeftChildIndex(int index) {
			if (index < 0 || index > getCount() - 1)
				throw new System.InvalidOperationException("Invalid index.");
			int result = (2 * index) + 1;
			if (result > getCount() - 1)
				result = index; // return itself if no children
			return result;
		}
		private static int getRightChildIndex(int index){
			if (index < 0 || index > getCount() - 1)
				throw new System.InvalidOperationException("Invalid index.");
			int result = (2 * index) + 2;
			if (result > getCount() - 1)
				result = index; // return itself if no children
			return result;
		}
		private static int getMin() {
			if (isEmpty()) throw new System.InvalidOperationException("simplifier is empty.");
			return s_content[0];
		}

		private static int extractMin() {
			int result = getMin();
			deleteMin();
			return result;
		}
		private static void deleteMin() {
			if (isEmpty()) throw new System.InvalidOperationException("simplifier is empty.");
			int pointIndexToRemove = s_content[0];
			s_removed.Add(pointIndexToRemove);
			switchItems(0, getCount() - 1);
			s_content.RemoveAt(getCount() - 1);
			s_NumberOfItems--;

			if (!isEmpty()) bubbleDown(0);

			int prevIndex = pointIndexToRemove-1;
			while(s_removed.Contains(prevIndex) && prevIndex > 0) prevIndex--;
			int pIdx = s_content.IndexOf(prevIndex);
			if (prevIndex > 0) bubbleDown(bubbleUp(pIdx));

			int nextIndex = pointIndexToRemove+1;
			while(s_removed.Contains(nextIndex) && nextIndex < s_Points.Count-1) nextIndex++;
			int nIdx = s_content.IndexOf(nextIndex);
			if (nextIndex < s_Points.Count-1) bubbleDown(bubbleUp(nIdx));
		}
		private static int insert(int pointIndex) {
			s_content.Add(pointIndex);
			int index = s_content.Count-1;
			s_NumberOfItems++;
			index = bubbleUp(index);
			return index;
		}

		private static bool isFirstBigger(int first, int second) {
			////Debug.Log("Comparing: "+first+" to "+second+" s_content.Count = "+getCount());
			return triangleArea(s_content[first]) > triangleArea(s_content[second]);
		}
		private static int bubbleUp(int index) {
			if (index == 0) return 0;
			int parent = getParentIndex(index);
			// while parent is smaller and item not on root already
			while (index != 0 && isFirstBigger(parent,index)) {
				switchItems(index, parent);
				index = parent;
				parent = getParentIndex(parent);
			}
			return index;
		}
		private static int bubbleDown(int index) {
			int leftChild, rightChild, targetChild;
			bool finished = false;
			while (!finished) {
				leftChild = getLeftChildIndex(index);
				rightChild = getRightChildIndex(index);
				// if left child is bigger then right child
				// when no children, get child will return element itself
				if (leftChild == index && rightChild == index) {
					finished = true; // bubbled down to the end
				}
				else {// bubble further
					//Get the smaller of the two children
					if (isFirstBigger(leftChild, rightChild)) targetChild = rightChild;
					else targetChild = leftChild;
					// if smaller item at index is bigger than smaller child
					if (isFirstBigger(index, targetChild)) {
						switchItems(targetChild, index);
						index = targetChild;
					}
					else finished = true;
				}
			}
			return index;
		}
		private static void switchItems(int index1, int index2) {
			int temp = s_content[index1];
			s_content[index1] = s_content[index2];
			s_content[index2] = temp;
		}

		private static float triangleArea(int pointIndex) {
			if (pointIndex <= 0 || pointIndex >= s_Points.Count-1) return Mathf.Infinity;
			else {
				Vector3 v1, v2;
				Vector3 a, b, c;
				int prevIndex = pointIndex-1;
				int nextIndex = pointIndex+1;
				while(s_removed.Contains(prevIndex) && prevIndex > 0) prevIndex--;
				while(s_removed.Contains(nextIndex) && nextIndex < s_Points.Count-1) nextIndex++;
				a = s_Points[prevIndex];
				b = s_Points[pointIndex];
				c = s_Points[nextIndex];

				if (s_PreserveAcute) {
					v1 = LineDirection(b, a);
					v2 = LineDirection(b, c);
					float angle = Vector3.Angle(v1, v2);
					if (angle < 1f) return Mathf.Infinity;
				}
				return Mathf.Abs(a.x*(b.y-c.y) + b.x*(c.y-a.y) + c.x*(a.y-b.y))/2;
			}
		}
		private static Vector3 LineDirection(Vector3 point1, Vector3 point2) {
			Vector3 direction = Vector3.zero;
			direction.x = point1.y-point2.y; direction.y = -(point1.x-point2.x);
			direction.Normalize();
			return direction;
		}
	}
}
