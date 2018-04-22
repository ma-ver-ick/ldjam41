using UnityEngine;

namespace ldjam41 {
    public class PauseMenuManager : MonoBehaviour {
        public KeyCode ActivationCode = KeyCode.Escape;
        public GameObject PauseMenu;

        public bool GamePaused;
        
        private void Start() {
            PauseMenu.SetActive(false);
        }

        private void Update() {
            if (!Input.GetKeyUp(ActivationCode)) {
                return;
            }

            GamePaused = !GamePaused;
            PauseMenu.SetActive(GamePaused);

            if (GamePaused) {
                Time.timeScale = 0.0f;
            }
            else {
                Time.timeScale = 1.0f;
            }
        }

        public void Close() {
            PauseMenu.SetActive(false);
        }
    }
}