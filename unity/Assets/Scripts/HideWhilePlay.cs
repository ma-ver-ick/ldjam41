using UnityEngine;

namespace ldjam41 {
    public class HideWhilePlay : MonoBehaviour {
        private void Start() {
            gameObject.SetActive(false);
        }
    }
}