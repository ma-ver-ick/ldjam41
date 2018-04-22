using UnityEngine;

namespace ldjam41 {
    [ExecuteInEditMode]
    public class ZombieTrapDebug : MonoBehaviour {
        public Transform Spawn01;
        public Transform Spawn02;

        private void Update() {
            Debug.DrawLine(Spawn01.position + Vector3.up * 0.1f, Spawn02.position + Vector3.up * 0.1f, Color.blue);
        }
    }
}