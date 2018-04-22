using UnityEngine;

namespace ldjam41 {
    public class ZombieTrapCrossingStreetBehaviour : ZombieTrapBehaviour {
        public Vector3 StartPosition;
        public Vector3 EndPosition;

        public override void TransformZombieForTime(float t) {
            // Trap.transform.localRotation = Quaternion.Euler(Vector3.Lerp(FromRotation, ToRotation, t));
            
            var dir = (EndPosition - StartPosition) * t;
            Trap.transform.position = StartPosition + dir;
        }

        public override void ResetZombie() {
            Trap.SetActive(false);
        }

        public override void ZombieStartPosition() {
            StartPosition = Spawn02.position;
            EndPosition = Spawn01.position;
            if (Random.Range(0.0f, 1.0f) < 0.5) {
                StartPosition = Spawn01.position;
                EndPosition = Spawn02.position;
            }

            Trap.transform.position = StartPosition;
            Trap.SetActiveRecursively(true);
        }
    }
}