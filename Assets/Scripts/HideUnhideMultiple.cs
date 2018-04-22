using UnityEngine;

namespace ldjam41 {
    public class HideUnhideMultiple : MonoBehaviour {
        public MonoBehaviour[] Elements;

        public void HideAll() {
            foreach (var e in Elements) {
                e.enabled = false;
            }
        }

        public void ShowAll() {
            foreach (var e in Elements) {
                e.enabled = true;
            }
        }
    }
}