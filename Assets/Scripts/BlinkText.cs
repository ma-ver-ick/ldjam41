using TMPro;
using UnityEngine;

namespace ldjam41 {
    public class BlinkText : MonoBehaviour {
        public TextMeshProUGUI Text;
        public float BlinkSpeedSeconds = 0.25f;
        public Color BlinkColor = Color.yellow;
        public Color NormalColor = Color.white;

        private bool _blinking = false;
        private float _blinkStart = -1.0f;


        public void StartBlink() {
            if (_blinking) {
                return;
            }

            _blinking = true;
            _blinkStart = Time.time;
        }

        public void StopBlink() {
            _blinking = false;
            Text.color = NormalColor;
        }

        private void Update() {
            if (!_blinking) {
                return;
            }

            var el = Mathf.FloorToInt((Time.time - _blinkStart) / BlinkSpeedSeconds);
            if (el % 2 == 0) {
                Text.color = BlinkColor;
            }
            else {
                Text.color = NormalColor;
            }
        }
    }
}