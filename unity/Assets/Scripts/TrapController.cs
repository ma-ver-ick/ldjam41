using System.Collections.Generic;
using UnityEngine;

namespace ldjam41 {
    public class TrapController : MonoBehaviour {
        public float Probability;
        public Collider FilterCollider;
        public RacingController RacingController;
        public RoadGenerator RoadGenerator;
        public RoadController RoadController;

        public GameObject[] Prefabs;

        public ZombieTrapBehaviour[] Zombies;
        public ZombieHitBehaviour[] ZombieHits;
        private List<GameObject> Traps = new List<GameObject>();

        private float _lastProbability = -1;
        private float _lastRoadGeneratorHash = -1;

        private void Start() { }

        private void Update() {
            if (!Mathf.Approximately(_lastRoadGeneratorHash, RoadGenerator.LastWaypointHash)) {
                UpdateTraps();
            }

            if (Mathf.Approximately(Probability, _lastProbability)) {
                return;
            }

            foreach (var z in Zombies) {
                z.Probability = Probability;
                z.FilterCollider = FilterCollider;
            }

            foreach (var z in ZombieHits) {
                z.RacingController = RacingController;
                z.FilterCollider = FilterCollider;
            }

            _lastProbability = Probability;
        }

        private void UpdateTraps() {
            // CleanUp
            if (Traps.Count > 0) {
                foreach (var t in Traps) {
                    Destroy(t);
                }
            }

            Traps.Clear();

            var lengthCovered = 0.0f;
            var i = 0;
            foreach (var ls in RoadController.Segments) {
                var length = ls.tEnd;
                lengthCovered += length;
                if (length < 16) {
                    continue;
                }

                var go = Instantiate(Prefabs[i++ % Prefabs.Length]);
                go.transform.position = ls.Start + 8*ls.Direction;
                go.transform.parent = transform;
                go.transform.rotation = Quaternion.LookRotation(ls.Direction);
                Traps.Add(go);

            }

            Zombies = GetComponentsInChildren<ZombieTrapBehaviour>(true);
            ZombieHits = GetComponentsInChildren<ZombieHitBehaviour>(true);
            _lastRoadGeneratorHash = RoadGenerator.LastWaypointHash;
        }
    }
}