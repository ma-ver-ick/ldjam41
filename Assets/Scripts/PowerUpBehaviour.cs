using UnityEngine;

namespace ldjam41 {
    public class PowerUpBehaviour : MonoBehaviour {
        public RacingController RacingController;
        public Collider FilterCollider;
        public AudioSource Sound;

        public float DisabledTime = 30.0f;
        public float LastHitTime = -1;

        private MeshRenderer Display;

        private void Start() {
            Sound = GetComponent<AudioSource>();
            Display = GetComponent<MeshRenderer>();
        }

        private void Update() {
            if (LastHitTime > 0 && Time.time - LastHitTime < DisabledTime) {
                return;
            }

            if (!Display.enabled) {
                Display.enabled = true;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other != FilterCollider) {
                return;
            }

            if (LastHitTime > 0 && Time.time - LastHitTime < DisabledTime) {
                return;
            }

            RacingController.OnPowerUpCollected();
            Sound.Play();
            LastHitTime = Time.time;
            Display.enabled = false;
        }
    }
}