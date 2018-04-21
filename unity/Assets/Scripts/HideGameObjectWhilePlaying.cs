using UnityEngine;

namespace ldjam41 {
    public class HideGameObjectWhilePlaying : MonoBehaviour {
        private void Start() {
            gameObject.SetActive(false);
        }
    }
}