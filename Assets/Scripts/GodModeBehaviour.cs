using UnityEngine;

namespace ldjam41 {
    public class GodModeBehaviour : MonoBehaviour {
        public bool GodMode;
        public string ActivationString = "idkfa";
        public RacingController RacingController;
        public TrapController TrapController;

        private int _index = 0;
        private char[] _cheatCode;

        void Start() {
            _cheatCode = ActivationString.ToCharArray();
            _index = 0;
        }

        void Update() {
            // Check if any key is pressed
            if (Input.anyKeyDown && _index < _cheatCode.Length) {
                // Check if the next key in the code is pressed
                if (Input.GetKeyDown("" + _cheatCode[_index])) {
                    // Add 1 to index to check the next key in the code
                    _index++;
                }
                // Wrong key entered, we reset code typing
                else {
                    _index = 0;
                }
            }

            // If index reaches the length of the cheatCode string, 
            // the entire code was correctly entered
            if (_index == _cheatCode.Length) {
                // Cheat code successfully inputted!
                // Unlock crazy cheat code stuff
                GodMode = true;
                TrapController.Probability = 0.0f;
                RacingController.WarningHits = 100;
                RacingController.MinSpeed = 0.0f;
                RacingController.SecondsTillDeath = 100;
            }
        }
    }
}