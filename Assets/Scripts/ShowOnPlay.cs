using UnityEngine;

namespace ldjam41 {
    public class ShowOnPlay : MonoBehaviour {
        public GameObject GameObject;

        private void Start() {
            GameObject.SetActive(true);
        }
    }
}