using UnityEngine;
using UnityEngine.SceneManagement;

namespace ldjam41 {
    public class SceneControls : MonoBehaviour {

        public void Restart() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ExitGame() {
            Application.Quit();
        }
        
    }
}