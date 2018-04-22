using System;
using UnityEngine;

namespace ldjam41 {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class RoadGenerator : MonoBehaviour {
        public Transform[] Waypoints;

        public Vector3[] CalculationWaypoints;

        public MeshFilter MeshFilter;
        public MeshCollider MeshCollider;

        public float Width = 10.0f;
        public float LengthDivier = 10.0f;
        public float CurveBlendingArea = 1.5f;

        public bool AlwaysUpdate;

        private int _lastWaypointCount = -1;
        private float _lastWaypointSum = -1;

        public float LastWaypointHash => _lastWaypointCount + _lastWaypointSum;

        private void Start() {
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();

            var ts = gameObject.GetComponentsInChildren<Transform>(true);
            Waypoints = new Transform[ts.Length - 1];
            var i = 0;
            foreach (var t in ts) {
                if (t.gameObject == gameObject) {
                    continue;
                }

                Waypoints[i++] = t;
            }
        }

        private void Update() {
            var shouldUpdate = AlwaysUpdate || _lastWaypointCount == -1 || _lastWaypointSum < 0 || HasChanged();

            if (shouldUpdate) {
                Debug.Log("Updating Road Mesh...");
                var mesh = GenerateMesh();
                MeshFilter.sharedMesh = mesh;
                MeshCollider.sharedMesh = mesh;
            }
        }

        private Mesh GenerateMesh() {
            var basicOffset = transform.position;

            using (var m = new Meshbuilder(10, 30)) {
                CalculationWaypoints = new Vector3[Waypoints.Length];

                var rot = Quaternion.Euler(0, 90, 0);

                var dummySum = 0.0f;
                var lengthSoFar = 0.0f;
                for (var i = 0; i < Waypoints.Length; i++) {
                    var wp = Waypoints[i];
                    var wpNext = Waypoints[(i + 1) % Waypoints.Length];
                    var wpNextNext = Waypoints[(i + 2) % Waypoints.Length];

                    CalculationWaypoints[i] = wp.position;
                    dummySum += wp.position.sqrMagnitude;

                    var dir = (wpNext.position - wp.position).normalized;
                    var left = -(rot * dir);

                    // Middle Segment
                    var startPosMiddle = wp.position - basicOffset + dir * CurveBlendingArea;
                    var startPosLeft = startPosMiddle + left * Width;
                    var startPosRight = startPosMiddle + -left * Width;

                    var endPosMiddle = (wpNext.position - basicOffset) - dir * CurveBlendingArea;
                    var endPosLeft = endPosMiddle + left * Width;
                    var endPosRight = endPosMiddle + -left * Width;
                    var length = (endPosMiddle - startPosMiddle).magnitude / LengthDivier;

                    var sl = m.AddVertex(startPosLeft, new Vector2(lengthSoFar, 0.0f));
                    var sr = m.AddVertex(startPosRight, new Vector2(lengthSoFar, 1.0f));
                    var el = m.AddVertex(endPosLeft, new Vector2(lengthSoFar + length, 0.0f));
                    var er = m.AddVertex(endPosRight, new Vector2(lengthSoFar + length, 1.0f));
                    m.AddQuad(el, er, sl, sr);
                    lengthSoFar += length;

                    // Blending Area Segment
                    var dirNext = (wpNextNext.position - wpNext.position).normalized;
                    var nextPosMiddle = (wpNext.position - basicOffset) + dirNext * CurveBlendingArea;
                    var leftNext = -(rot * dirNext);
                    var nextPosLeft = nextPosMiddle + leftNext * Width;
                    var nextPosRight = nextPosMiddle + -leftNext * Width;
                    length = CurveBlendingArea / 5.0f; //(nextPosMiddle - endPosMiddle).magnitude; TODO WHY?

                    sl = m.AddVertex(endPosLeft, new Vector2(lengthSoFar, 0.0f));
                    sr = m.AddVertex(endPosRight, new Vector2(lengthSoFar, 1.0f));
                    el = m.AddVertex(nextPosLeft, new Vector2(lengthSoFar + length, 0.0f));
                    er = m.AddVertex(nextPosRight, new Vector2(lengthSoFar + length, 1.0f));
                    m.AddQuad(el, er, sl, sr);

                    lengthSoFar += length;
                }

                _lastWaypointCount = Waypoints.Length;
                _lastWaypointSum = dummySum;
                return m.GenerateMesh();
            }
        }

        private bool HasChanged() {
            if (_lastWaypointCount != Waypoints.Length) {
                return true;
            }

            var dummy = 0.0f;
            for (var i = 0; i < Waypoints.Length; i++) {
                var wp = Waypoints[i].position;
                dummy += wp.sqrMagnitude;
            }

            return !Mathf.Approximately(dummy, _lastWaypointSum);
        }
    }
}