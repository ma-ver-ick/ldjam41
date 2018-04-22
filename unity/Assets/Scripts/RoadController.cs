using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;

namespace ldjam41 {
    [RequireComponent(typeof(RoadGenerator))]
    public class RoadController : MonoBehaviour {
        public Transform Car;

        public float AllowedDistanceToRoad = 5.0f;
        public float AllowedAngle = 60.0f;

        public bool HighlightRoad;
        public Transform RoadMarker;

        public float DebugDistance;
        public float DebugAngle;

        private RoadGenerator RoadGenerator;

        public bool OffTrack => AllowedDistanceToRoad < DebugDistance;
        public bool WrongDirection => Mathf.Abs(DebugAngle) > AllowedAngle;

        private float _lastWaypointChange = -1;
        public LineSegment[] Segments;
        public SimpleTree SimpleTree = new SimpleTree();

        private void Start() {
            RoadGenerator = GetComponent<RoadGenerator>();
        }

        private void Update() {
            SimpleTree.DebugDraw();
            if (RoadGenerator.LastWaypointHash < 0) {
                return;
            }

            if (!Mathf.Approximately(_lastWaypointChange, RoadGenerator.LastWaypointHash)) {
                UpdateSegments();
            }

            DebugDistance = CarDistanceToCenterRoad();
            DebugAngle = CarAngle();

            if (HighlightRoad) {
                var s = GetNearestSegment(Car.position);
                RoadMarker.transform.position = s.Start + Vector3.up * 5;
            }
        }

        public float CarDistanceToCenterRoad() {
            var s = GetNearestSegment(Car.position);
            return (Car.position - s.Project(Car.position)).magnitude;
        }

        public float CarAngle() {
            var s = GetNearestSegment(Car.position);

            Debug.DrawLine(Car.position, Car.position + Car.forward * 100, Color.green);
            return Vector3.SignedAngle(Car.forward, s.Direction, Vector3.up);
        }

        public LineSegment GetNearestSegment(Vector3 carPosition) {
            var ret = float.MaxValue;
            var retSegment = Segments[0];
            for (var i = 0; i < Segments.Length; i++) {
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

        public LineSegment GetNearestSegment(Vector3 carPosition, LineSegment[] segements) {
            var ret = float.MaxValue;
            var retSegment = segements[0];
            for (var i = 0; i < segements.Length; i++) {
                var s = segements[i];

                var pp = s.Project(carPosition);
                var dist = (carPosition - pp).magnitude;
                if (dist < ret) {
                    retSegment = s;
                    ret = dist;
                }
            }

            return retSegment;
        }

        public LineSegment GetNearestSegment(Vector3 carPosition, int startIdx, out int foundIdx) {
            var ret = float.MaxValue;
            var retSegment = Segments[0];
            foundIdx = -1;
            for (var i = Mathf.Max(startIdx, 0); i < Segments.Length; i++) {
                var s = Segments[i];

                var pp = s.Project(carPosition);
                var dist = (carPosition - pp).magnitude;
                if (dist < ret) {
                    retSegment = s;
                    ret = dist;
                    foundIdx = i;
                }
            }

            return retSegment;
        }

        private void UpdateSegments() {
            _lastWaypointChange = RoadGenerator.LastWaypointHash;
            SimpleTree.Clear();

            var rot = Quaternion.Euler(0, 90, 0);
            var width = RoadGenerator.Width;

            // convert waypoints to line segments
            var wps = RoadGenerator.CalculationWaypoints;
            Segments = new LineSegment[wps.Length];
            for (var i = 0; i < wps.Length; i++) {
                var w = wps[i];
                var wNext = wps[(i + 1) % wps.Length];

                // RECT
                var dir = (wNext - w).normalized;
                var left = -(rot * dir);

                var startPosMiddle = w;
                var startPosLeft = startPosMiddle + left * width;
                var startPosRight = startPosMiddle + -left * width;

                var endPosMiddle = wNext;
                var endPosLeft = endPosMiddle + left * width;
                var endPosRight = endPosMiddle + -left * width;

                var rect = new Bounds();
                rect.SetMinMax(FindMin(startPosLeft, startPosRight, endPosLeft, endPosRight), FindMax(startPosLeft, startPosRight, endPosLeft, endPosRight));

                Segments[i] = new LineSegment(w, wNext, rect);
                SimpleTree.Add(Segments[i]);
            }
        }

        private Vector3 FindMin(params Vector3[] v) {
            var minX = v[0].x;
            for (var i = 1; i < v.Length; i++) {
                minX = Mathf.Min(minX, v[i].x);
            }

            var minZ = v[0].z;
            for (var i = 1; i < v.Length; i++) {
                minZ = Mathf.Min(minZ, v[i].z);
            }

            return new Vector3(minX, v[0].y - 1, minZ);
        }

        private Vector3 FindMax(params Vector3[] v) {
            var maxX = v[0].x;
            for (var i = 1; i < v.Length; i++) {
                maxX = Mathf.Max(maxX, v[i].x);
            }

            var maxZ = v[0].z;
            for (var i = 1; i < v.Length; i++) {
                maxZ = Mathf.Max(maxZ, v[i].z);
            }

            return new Vector3(maxX, v[0].y + 1, maxZ);
        }
    }

    public struct LineSegment {
        public readonly Vector3 Start;
        public readonly Vector3 End;
        public readonly Vector3 Direction;
        public readonly float tEnd;
        public readonly Bounds Rect;

        public LineSegment(Vector3 start, Vector3 end, Bounds rect) {
            Start = start;
            End = end;

            var diff = end - start;
            Direction = diff.normalized;

            tEnd = Mathf.Max(diff.x / Direction.x, Mathf.Max(diff.y / Direction.y, diff.z / Direction.z));
            Rect = rect;
        }

        public Vector3 Project(Vector3 point, bool clamped = true) {
            var t = Vector3.Dot(point - Start, Direction);
            t = clamped ? Mathf.Max(0, Mathf.Min(t, tEnd)) : t;

            return Start + t * Direction;
        }

        public bool IsOn(Vector3 projectedPoint) {
            // use t value of the projected point to determine if it is between start / end
            var tPoint = f_inv(projectedPoint);
            if (float.IsNaN(tPoint)) {
                return false;
            }

            return tPoint >= 0 && tPoint <= tEnd;
        }

        public float f_inv(Vector3 y) {
            var v1 = (y - Start);
            var compT = new Vector3(v1.x / Direction.x, v1.y / Direction.y, v1.z / Direction.z);

            var min = Mathf.Min(compT.x, Mathf.Min(compT.y, compT.z));
            var max = Mathf.Max(compT.x, Mathf.Max(compT.y, compT.z));

            var insideSegment = min <= tEnd && min >= 0;
            if (Mathf.Approximately(min, max) && insideSegment) {
                return min;
            }

            return float.NaN;
        }
    }
}