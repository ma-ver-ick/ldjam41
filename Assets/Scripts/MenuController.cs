using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ldjam41 {
    public class MenuController : MonoBehaviour {

        public SceneAsset RacingLevel;

        public GameObject TitleScreen;
        public GameObject IntroductionScreen;

        private void Start() {
            TitleScreen.SetActive(true);
            IntroductionScreen.SetActive(false);
        }

        public void StartGame() {
            SceneManager.LoadScene(RacingLevel.name);
        }

        public void ExitGame() {
            Application.Quit();
        }

        public void SwitchToIntroduction() {
            TitleScreen.SetActive(false);
            IntroductionScreen.SetActive(true);
        }

    }
}