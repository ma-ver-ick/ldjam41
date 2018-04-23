using UnityEngine;

namespace ldjam41 {
    [ExecuteInEditMode]
    public class RotateObject : MonoBehaviour {
        public bool ConstrainX;
        public bool ConstrainY;
        public bool ConstrainZ;

        public float Speed;
        private float _passedTime;

        
        private void Update() {
            _passedTime += Time.deltaTime;
            var rot = _passedTime * Speed;

            var temp = transform.localEulerAngles;
            var x = temp.x;
            var y = temp.y;
            var z = temp.z;

            if (!ConstrainX) {
                x = rot;
            }

            if (!ConstrainY) {
                y = rot;
            }

            if (!ConstrainZ) {
                z = rot;
            }

            transform.localEulerAngles = new Vector3(x, y, z);
        }
    }
}