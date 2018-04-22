using UnityEngine;

namespace ldjam41 {
    public class ZombieTrapBehaviour : MonoBehaviour {
        public Collider FilterCollider;
        public GameObject Trap;
        public GameObject ZombieHidePart;

        public float Probability;
        public float Speed;

        public Collider ZombieCollider;

        private float StartTime;
        private bool ActiveTrap;

        public Transform Spawn01;
        public Transform Spawn02;

        private void Update() {
            if (!ActiveTrap) {
                return;
            }

            var t = (Time.time - StartTime) * Speed;
            TransformZombieForTime(t);

            if (t > 1.0f) {
                ActiveTrap = false;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (FilterCollider != other) {
                return;
            }

            // think about raising the trap!

            var rnd = Random.Range(0.0f, 1.0f);
            if (rnd < Probability) {
                ActiveTrap = true;
                StartTime = Time.time;
                ZombieCollider.enabled = true;

                ZombieStartPosition();

                Debug.Log("Vehicle entered - RAISING TRAP " + rnd);
            }
            else {
                Debug.Log("Vehicle entered - not raising trap" + rnd);
            }
        }


        private void OnTriggerExit(Collider other) {
            if (FilterCollider != other) {
                return;
            }

            ZombieCollider.enabled = false;
            ResetZombie();
        }

        public virtual void ZombieStartPosition() { }
        public virtual void TransformZombieForTime(float t) { }
        public virtual void ResetZombie() { }

        public void OnZombieHit() {
            ZombieHidePart.SetActive(false);
        }
    }
}