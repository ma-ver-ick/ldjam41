using UnityEngine;

namespace ldjam41 {
    public class PowerUpController : MonoBehaviour {
        public RacingController RacingController;
        public Collider FilterCollider;

        private void Start() {
            var pups = GetComponentsInChildren<PowerUpBehaviour>();

            foreach (var pup in pups) {
                pup.RacingController = RacingController;
                pup.FilterCollider = FilterCollider;
            }
            Debug.LogError("Found " + pups.Length + " PowerUps below me.");
        }
    }
}