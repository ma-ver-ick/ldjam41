using UnityEngine;

namespace ldjam41 {
    public class ZombieTrapBottomBehaviour : ZombieTrapBehaviour {
        public Vector3 FromRotation;
        public Vector3 ToRotation;

        public override void TransformZombieForTime(float t) {
            Trap.transform.localRotation = Quaternion.Euler(Vector3.Lerp(FromRotation, ToRotation, t));
        }

        public override void ResetZombie() {
            Trap.transform.localRotation = Quaternion.Euler(FromRotation);
            Trap.SetActive(false);
        }
        
        public override void ZombieStartPosition() {
            var dir = (Spawn01.position - Spawn02.position) * Random.RandomRange(0.0f, 0.1f);
            Trap.transform.position = Spawn02.position + dir;
            
            Trap.SetActiveRecursively(true);
        }
    }
}