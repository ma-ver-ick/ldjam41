using UnityEngine;

namespace ldjam41 {
    [RequireComponent(typeof(RoadGenerator))]
    public class RoadController : MonoBehaviour {
        public Transform Car;

        public float DebugDistance;
        public float DebugAngle;

        private RoadGenerator RoadGenerator;

        private float _lastWaypointChange = -1;
        private LineSegment[] _segments;

        private void Start() {
            RoadGenerator = GetComponent<RoadGenerator>();
        }

        private void Update() {
            if (!Mathf.Approximately(_lastWaypointChange, RoadGenerator.LastWaypointHash)) {
                UpdateSegments();
            }

            DebugDistance = CarDistanceToCenterRoad();
            DebugAngle = CarAngle();
        }

        public float CarDistanceToCenterRoad() {
            var s = GetNearestSegment();
            return (Car.position - s.Project(Car.position)).magnitude;
        }

        public float CarAngle() {
            var s = GetNearestSegment();

            return Vector3.SignedAngle(Car.forward, s.Direction, Vector3.up);
        }

        public LineSegment GetNearestSegment() {
            var carPosition = Car.position;
            var ret = float.MaxValue;
            var retSegment = _segments[0];
            for (var i = 0; i < _segments.Length; i++) {
                var s = _segments[i];

                var pp = s.Project(carPosition);
                var dist = (carPosition - pp).magnitude;
                if (dist < ret) {
                    retSegment = s;
                    ret = dist;
                }
            }

            return retSegment;
        }

        private void UpdateSegments() {
            _lastWaypointChange = RoadGenerator.LastWaypointHash;

            // convert waypoints to line segments
            var wps = RoadGenerator.CalculationWaypoints;
            _segments = new LineSegment[wps.Length];
            for (var i = 0; i < wps.Length; i++) {
                var w = wps[i];
                var wNext = wps[(i + 1) % wps.Length];

                _segments[i] = new LineSegment(w, wNext);
            }
        }
    }

    public struct LineSegment {
        public readonly Vector3 Start;
        public readonly Vector3 End;
        public readonly Vector3 Direction;
        public readonly float tEnd;

        public LineSegment(Vector3 start, Vector3 end) {
            Start = start;
            End = end;

            var diff = end - start;
            Direction = diff.normalized;

            tEnd = Mathf.Max(diff.x / Direction.x, Mathf.Max(diff.y / Direction.y, diff.z / Direction.z));
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