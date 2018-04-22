using UnityEngine;

namespace ldjam41 {
    public class DisableScriptWhilePlaying : MonoBehaviour {
        public MeshRenderer ToDisable;

        private void Start() {
            ToDisable.enabled = false;
        }
    }
}