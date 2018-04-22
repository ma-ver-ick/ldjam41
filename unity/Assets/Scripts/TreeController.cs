using System.Collections.Generic;
using UnityEngine;

namespace ldjam41 {
    public class TreeController : MonoBehaviour {
        public GameObject Player;
        public TerrainData TerrainData;
        public RoadController RoadController;
        public RoadGenerator RoadGenerator;

        public GameObject[] Prefabs;

        public List<GameObject> Trees = new List<GameObject>();

        public GameObject Test;

        public Vector2 PerlinScale;
        public float TreeSpacing = 100.0f;
        public float Area = 20.0f;
        public int MaxTrees = 100;
        public float Hysteresis = 1.0f;
        public float RoadClearance = 8.0f;

        private Vector3 LastPlayerPosition = Vector3.negativeInfinity;

        private void Start() {
            for (var i = 0; i < MaxTrees; i++) {
                var go = Instantiate(Prefabs[i % Prefabs.Length]);
                go.transform.parent = transform;
                go.SetActive(false);
                Trees.Add(go);
            }
        }

        private void Update() {
            var player = Player.transform.position;
            if ((LastPlayerPosition - player).sqrMagnitude < Hysteresis) {
                return;
            }

            LastPlayerPosition = player;

            // deactive any tree thats too far away
            foreach (var t in Trees) {
                if (!t.activeSelf) {
                    continue;
                }

                if ((t.transform.position - player).magnitude > Area) {
                    t.SetActive(false);
                }
            }

            var segments = RoadController.Segments;
            if (segments == null || segments.Length == 0) {
                return;
            }

            var x = Mathf.FloorToInt(player.x);
            var y = Mathf.FloorToInt(player.z);

            var lastInactiveTree = 0;
            for (var i = Area; i > -Area; i--) {
                for (var ii = Area; ii > -Area; ii--) {
                    var pos = new Vector3(x + i, 0, y + ii);
                    if (Mathf.PerlinNoise(pos.x * PerlinScale.x, pos.z * PerlinScale.y) > 0.5f) {
                        continue;
                    }

                    if ((pos - player).magnitude > Area) {
                        continue;
                    }

                    var s = RoadController.GetNearestSegment(pos);
                    var dist = (pos - s.Project(pos)).magnitude;
                    if (dist < RoadClearance) {
                        continue;
                    }

                    pos += new Vector3(0, TerrainData.GetHeight(x, y), 0);
                    var canPlace = true;
                    foreach (var t in Trees) {
                        var distTree = (t.transform.position - pos).sqrMagnitude;
                        if (distTree < TreeSpacing) {
                            canPlace = false;
                            break;
                        }
                    }

                    if (!canPlace) {
                        continue;
                    }

                    for (; lastInactiveTree < Trees.Count; lastInactiveTree++) {
                        if (!Trees[lastInactiveTree].activeSelf) {
                            break;
                        }
                    }

                    if (lastInactiveTree >= Trees.Count) {
                        break; // no more trees available
                    }

                    var go = Trees[lastInactiveTree];
                    go.transform.position = pos;
                    go.transform.parent = transform;
                    go.SetActive(true);
                }
            }
        }
    }
}