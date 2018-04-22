using UnityEngine;

namespace ldjam41 {
    public class StartFinishTrigger : MonoBehaviour {
        public RacingController RacingController;
        public Collider FilterCollider;

        private void OnTriggerExit(Collider other) {
            if (FilterCollider != other) {
                return;
            }

            RacingController.OnStartFinishTrigger();
        }
    }
}