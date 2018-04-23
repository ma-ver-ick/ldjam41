using System;
using TMPro;
using UnityEngine;

namespace ldjam41 {
    public class QualitySwitcher : MonoBehaviour {
        public string[] QualityNames;
        public TMP_Dropdown Dropdown;

        private void Start() {
            QualityNames = QualitySettings.names;

            Dropdown.options.Clear();
            foreach (var q in QualityNames) {
                var o = new TMP_Dropdown.OptionData {text = q};

                Dropdown.options.Add(o);
            }

            Dropdown.value = QualitySettings.GetQualityLevel();
        }

        private void Update() { }

        public void SetQualityLevel(int lvl) {
            QualitySettings.SetQualityLevel(lvl, true);
        }
    }
}