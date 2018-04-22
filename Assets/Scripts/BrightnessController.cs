using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

namespace ldjam41 {
    public class BrightnessController : MonoBehaviour {
        public PostProcessingProfile PostProcessingProfile;
        public Slider Slider;

        public float Brightness = 0.0f;
        private float _lastBrightness = 0.0f;

        private const string BRIGHTNESS_KEY = "BRIGHTNESS";

        private void Start() {
            Brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, 0.0f);
            Slider.value = Brightness;
        }

        public void SetBrightness(float value) {
            // PlayerPrefs.SetFloat(BRIGHTNESS_KEY, value);
            Brightness = value;
        }

        private void Update() {
            if (!Mathf.Approximately(_lastBrightness, Brightness)) {
                var colorGradingSettings = PostProcessingProfile.colorGrading.settings;
                colorGradingSettings.basic.postExposure = Brightness;
                PostProcessingProfile.colorGrading.settings = colorGradingSettings;

                _lastBrightness = Brightness;
                PlayerPrefs.SetFloat(BRIGHTNESS_KEY, Brightness);
            }
        }
    }
}