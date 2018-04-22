using UnityEngine;

namespace ldjam41 {
    public class ZombieHitBehaviour : MonoBehaviour {
        public RacingController RacingController;
        public Collider FilterCollider;
        public ZombieTrapBehaviour ZombieTrap;
        public AudioSource HitSound;
        
        private void OnTriggerEnter(Collider other) {
            if (other != FilterCollider) {
                return;
            }
            HitSound.Play();

            RacingController.OnZombieHit();
            ZombieTrap.OnZombieHit();
        }

    }
}