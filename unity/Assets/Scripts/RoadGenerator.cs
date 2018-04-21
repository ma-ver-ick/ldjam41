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

        public bool AlwaysUpdate;

        private int _lastWaypointCount = -1;
        private float _lastWaypointSum = -1;

        public float LastWaypointHash => _lastWaypointCount + _lastWaypointSum;

        private void Start() {
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
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

                var dummySum = 0.0f;
                var lengthSoFar = 0.0f;
                for (var i = 0; i < Waypoints.Length; i++) {
                    var wp = Waypoints[i];
                    CalculationWaypoints[i] = wp.position;
                    dummySum += wp.position.sqrMagnitude;

                    var startPosMiddle = wp.position - basicOffset;
                    var startPosLeft = startPosMiddle + wp.rotation * Vector3.left * Width;
                    var startPosRight = startPosMiddle + wp.rotation * Vector3.right * Width;

                    var wpNext = Waypoints[(i + 1) % Waypoints.Length];
                    var endPosMiddle = wpNext.position - basicOffset;
                    var endPosLeft = endPosMiddle + wp.rotation * Vector3.left * Width;
                    var endPosRight = endPosMiddle + wp.rotation * Vector3.right * Width;

                    var length = (endPosMiddle - startPosMiddle).magnitude / LengthDivier;

                    var sl = m.AddVertex(startPosLeft, new Vector2(lengthSoFar, 0.0f));
                    var sr = m.AddVertex(startPosRight, new Vector2(lengthSoFar, 1.0f));
                    var el = m.AddVertex(endPosLeft, new Vector2(lengthSoFar + length, 0.0f));
                    var er = m.AddVertex(endPosRight, new Vector2(lengthSoFar + length, 1.0f));

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