using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        private bool _coroutineRunning;

        private void Start() {
            for (var i = 0; i < MaxTrees; i++) {
                var go = Instantiate(Prefabs[i % Prefabs.Length]);
                go.transform.parent = transform;
                go.SetActive(false);
                Trees.Add(go);
            }
        }

//        // no one has to see this :( 
//        List<LineSegment> lastLineSegments = new List<LineSegment>(15);
//        Dictionary<LineSegment, LineSegment> knownLineSegments = new Dictionary<LineSegment, LineSegment>();

        private void Update() {
            var segments = RoadController.Segments;
            if (segments == null || segments.Length == 0) {
                return;
            }

            var player = Player.transform.position;
            if ((LastPlayerPosition - player).sqrMagnitude < Hysteresis) {
                return;
            }

            LastPlayerPosition = player;

            if (!_coroutineRunning) {
                StartCoroutine(TreeUpdate());
            }
        }

        private IEnumerator TreeUpdate() {
            _coroutineRunning = true;
            var sw = Stopwatch.StartNew();

            var player = Player.transform.position;
            // deactive any tree thats too far away
            foreach (var t in Trees) {
                if (!t.activeSelf) {
                    continue;
                }

                if ((t.transform.position - player).magnitude > Area) {
                    t.SetActive(false);
                }
            }


            var x = Mathf.FloorToInt(player.x);
            var y = Mathf.FloorToInt(player.z);

            LineSegment? lastLineSegment = null;

            var lastInactiveTree = 0;
            for (var i = Area; i > -Area; i--) {
                for (var ii = Area; ii > -Area; ii--) {
                    // finish coroutine if it takes longer than 1/60s
                    if (sw.ElapsedMilliseconds >= 15) {
                        yield return null;
                        sw.Restart();
                    }


                    var pos = new Vector3(x + i, 0, y + ii);
                    if (Mathf.PerlinNoise(pos.x * PerlinScale.x, pos.z * PerlinScale.y) > 0.5f) {
                        continue;
                    }

                    if ((pos - player).magnitude > Area) {
                        continue;
                    }

                    if (lastLineSegment.HasValue) {
                        var dist2 = (pos - lastLineSegment.Value.Project(pos)).magnitude;
                        if (dist2 < RoadClearance) {
                            continue;
                        }
                    }

                    LineSegment? sTemp = null;
                    sTemp = RoadController.SimpleTree.GetNearestSegment(pos);
                    if (!sTemp.HasValue) {
                        continue;
                    }

                    var s = sTemp.Value;
                    lastLineSegment = s;


                    var dist = (pos - s.Project(pos)).magnitude;
                    if (dist < RoadClearance) {
                        continue;
                    }

                    pos += new Vector3(0, 0, 0); // TerrainData.GetHeight(x, y)
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

            // completed;
            _coroutineRunning = false;
        }
    }
}