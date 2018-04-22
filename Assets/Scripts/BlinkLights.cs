using System.Diagnostics;
using UnityEngine;

namespace ldjam41 {
    public class BlinkLights : MonoBehaviour {
        public AnimationCurve BlinkCurve;
        public float BlinkCurveDuration;

        public float BlinkInterval;

        private float TimeLastBlink;
        private bool Blinking;

        public Light[] Lights;

        private void Start() {
            TimeLastBlink = Time.time;
            SetIntensity(1.0f);
        }

        private void Update() {
            var el = Time.time - TimeLastBlink;
            if (!Blinking) {
                if (el > BlinkInterval) {
                    TimeLastBlink = Time.time;
                    Blinking = true;
                }

                return;
            }

            el = el / BlinkCurveDuration;

            var intensity = BlinkCurve.Evaluate(el);
            SetIntensity(intensity);

            if (el > 1.0f) {
                Blinking = false;
            }
        }

        private void SetIntensity(float intensity) {
            foreach (var l in Lights) {
                l.intensity = intensity;
            }
        }
    }
}