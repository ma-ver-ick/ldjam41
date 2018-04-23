using UnityEngine;

namespace ldjam41 {
    public class PowerUpBehaviour : MonoBehaviour {
        public RacingController RacingController;
        public Collider FilterCollider;
        public AudioSource Sound;

        public float DisabledTime = 30.0f;
        private float _lastHitTime = -1;

        private MeshRenderer Display;

        private void Start() {
            _lastHitTime = -1;
            Sound = GetComponent<AudioSource>();
            Display = GetComponent<MeshRenderer>();
        }

        private void Update() {
            if (_lastHitTime > 0 && Time.time - _lastHitTime < DisabledTime) {
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

            if (_lastHitTime > 0 && Time.time - _lastHitTime < DisabledTime) {
                return;
            }

            RacingController.OnPowerUpCollected();
            Sound.Play();
            _lastHitTime = Time.time;
            Display.enabled = false;
        }
    }
}