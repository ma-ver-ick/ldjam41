using System.Collections.Generic;
using UnityEngine;

namespace ldjam41 {
    public class SimpleTree {
        public List<Node> Nodes = new List<Node>();

        public void Add(LineSegment ls) {
            var rect = ls.Rect;

            var found = false;
            foreach (var node in Nodes) {
                var ins = node.Bounds.Intersects(rect);
                var dist = node.Bounds.SqrDistance(rect.center);
                if (ins && node.Segments.Count < 10) { // || (dist < 10.0f && node.Segments.Count < 10)
                    found = true;
                    node.Segments.Add(ls);
                    node.Bounds.Encapsulate(rect);
                    break;
                }
            }

            if (found) {
                return;
            }

            var n = new Node();
            n.Bounds = rect;
            n.Segments.Add(ls);
            Nodes.Add(n);
        }

        public void DebugDraw() {
            foreach (var node in Nodes) {
                var mi = node.Bounds.min;
                var ma = node.Bounds.max;
                Debug.DrawLine(new Vector3(mi.x, mi.y, mi.z), new Vector3(mi.x, mi.y, ma.z), Color.magenta);
                Debug.DrawLine(new Vector3(ma.x, mi.y, mi.z), new Vector3(ma.x, mi.y, ma.z), Color.magenta);


                Debug.DrawLine(new Vector3(mi.x, mi.y, mi.z), new Vector3(ma.x, mi.y, mi.z), Color.magenta);
                Debug.DrawLine(new Vector3(mi.x, mi.y, ma.z), new Vector3(ma.x, mi.y, ma.z), Color.magenta);

                Debug.DrawLine(mi, ma, Color.magenta);
            }
        }

        public void Clear() {
            Nodes.Clear();
        }

        public LineSegment? GetNearestSegment(Vector3 carPosition) {
            // Search INSIDE bounding box
            var ret = float.MaxValue;
            LineSegment? retSegment = null;
            var nodeCount = Nodes.Count;
            for (var i = 0; i < nodeCount; i++) {
                var n = Nodes[i];
                if (!n.Bounds.Contains(carPosition)) {
                    continue;
                }

                var segmentCount = n.Segments.Count;
                for (var ii = 0; ii < segmentCount; ii++) {
                    var s = n.Segments[ii];

                    var pp = s.Project(carPosition);
                    var dist = (carPosition - pp).magnitude;
                    if (dist < ret) {
                        retSegment = s;
                        ret = dist;
                    }
                }
            }

            if (retSegment.HasValue) {
                return retSegment;
            }

            // Search OUTSIDE bounding box
            // - Find NEAREST bounding box
            Node nearestNode = null;
            var nearestSD = float.MaxValue;
            for (var i = 0; i < nodeCount; i++) {
                var n = Nodes[i];
                var d = n.Bounds.SqrDistance(carPosition);
                if (nearestSD > d) {
                    nearestNode = n;
                    nearestSD = d;
                }
            }

            if (nearestNode == null) {
                Debug.Log("WRONG IMPLEMENTATION");
                return null;
            }

            // find nearest line segment
            return nearestNode.GetNearestSegment(carPosition, false);
        }
    }

    public class Node {
        // public Node ChildNode;
        public Bounds Bounds;
        public List<LineSegment> Segments = new List<LineSegment>();

        public LineSegment? GetNearestSegment(Vector3 carPosition, bool boundsCheck = true) {
            if (Segments.Count == 0) {
                return null;
            }

            if (!boundsCheck && !Bounds.Contains(carPosition)) {
                return null;
            }

            var ret = float.MaxValue;
            LineSegment? retSegment = null;
            var segmentCount = Segments.Count;
            for (var i = 0; i < segmentCount; i++) {
                var s = Segments[i];

                var pp = s.Project(carPosition);
                var dist = (carPosition - pp).magnitude;
                if (dist < ret) {
                    retSegment = s;
                    ret = dist;
                }
            }

            return retSegment;
        }
    }
}