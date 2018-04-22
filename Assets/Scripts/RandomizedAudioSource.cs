using System.Runtime.CompilerServices;
using UnityEngine;

namespace ldjam41 {
    public class RandomizedAudioSource : MonoBehaviour {
        public AudioSource Player;
        public AudioClip[] Clips;

        public AnimationCurve FadeoutCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);
        public float FadeoutTime = 2.0f;

        private float _fadeoutVolumeStart = -1.0f;
        private float _fadeoutTimeStart;
        private bool _playing;
        private bool _fadeout;

        public void Play(float volume) {
            if (_fadeout) {
                // stop fade, force playing
                _playing = false;
                _fadeout = false;
            }
            
            if (_playing) {
                return;
            }

            InternalSwitchClipAndPlay();
            _playing = true;
            _fadeout = false;
            Player.volume = volume;
            Debug.Log("Play Zombies " + volume);
        }

        private void InternalSwitchClipAndPlay() {
            var c = Random.Range(0, Clips.Length);
            Player.clip = Clips[c];
            Player.loop = false;
            Player.Play();
        }

        public void Stop() {
            if (!_playing) {
                return;
            }

            Player.Stop();
            _playing = false;
            _fadeout = false;
        }

        public void Fadeout() {
            if (!_playing || _fadeout) {
                return;
            }

            _fadeoutVolumeStart = Player.volume;
            _fadeoutTimeStart = Time.time;
            _fadeout = true;
        }

        public void SetVolume(float volume) {
            Player.volume = volume;
        }

        private void Update() {
            if (!_playing) {
                return;
            }

            if (!Player.isPlaying) {
                InternalSwitchClipAndPlay();
            }

            if (_fadeout) {
                var el = (Time.time - _fadeoutTimeStart) / FadeoutTime;
                var vol = FadeoutCurve.Evaluate(el) * _fadeoutVolumeStart;
                //Debug.Log("New fadeout volume " + vol);
                Player.volume = vol;
                if (Player.volume < 0.1f) {
                    Player.Stop();
                }
            }
        }
    }
}