using TMPro;
using UnityEngine;

namespace ldjam41 {
    public class ScrollText : MonoBehaviour {
        public TextMeshProUGUI Text;

        public float Speed = 1.0f;

        public Vector2 Scroll;

        private bool _started;
        private float _timeStarted;

        private void Start() {
            StartScrolling();
        }

        private void Update() {
            if (!_started) {
                return;
            }

            var el = Time.time - _timeStarted;
            var t = el * Speed * (Scroll.y - Scroll.x) + Scroll.x;

            var textMargin = Text.margin;
            textMargin.y = t;
            Text.margin = textMargin;

            if (t < Scroll.y) {
                _started = false;
            }
        }

        public void StartScrolling() {
            _started = true;
            _timeStarted = Time.time;
        }

        public void ResetScrolling() {
            var textMargin = Text.margin;
            textMargin.y = Scroll.x;
            Text.margin = textMargin;

            _started = false;
            _timeStarted = -1;
        }
    }
}