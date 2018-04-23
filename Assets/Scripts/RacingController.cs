using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.Vehicles.Car;

namespace ldjam41 {
    // Countdown -> Racing
    public class RacingController : MonoBehaviour {
        public RoadController RoadController;

        public RacingState CurrentState;

        public RacingState StateCountdown;
        public RacingState StateRacing;
        public RacingState StateDead;
        public RacingState StateWon;

        public CarUserControl CarUserControl;
        public CarController CarController;
        public TextMeshProUGUI InfoDisplay;
        public TextMeshProUGUI SpeedDisplay;
        public TextMeshProUGUI TimeDisplay;
        public TextMeshProUGUI LapDisplay;
        public TextMeshProUGUI NormalInfoDisplay;
        public BlinkText SpeedDisplayBlink;
        public TextMeshProUGUI WarningMessageDisplay;

        public AudioSource FinalRoundAudio;
        public AudioSource NewBestTime;
        public AudioSource ZombieEatingAudio;

        public int Hits;
        public Light LightLeft;
        public Light LightRight;
        public float[] LightFallOff;

        public AnimationCurve WarningIntensity;
        public float SecondsTillDeath = 5.0f;
        public RandomizedAudioSource WarningZombie;

        public float MinSpeed = 15.0f;

        public HideUnhideMultiple DeathScreen;
        public HideUnhideMultiple SuccessScreen;

        public int LapsToWin = 3;
        public int WarningHits = 6;

        public PauseMenuManager PauseMenuManager;

        private void Start() {
            CurrentState = StateCountdown = new RacingStateCountdown();
            CurrentState.Start(this);

            StateRacing = new RacingStateRacing();
            StateDead = new RacingStateDead();
            StateWon = new RacingStateWon();

            ForceClear();
        }

        public void ForceClear() {
            InfoDisplay.text = "";
            TimeDisplay.text = "";
            WarningMessageDisplay.text = "";
            SpeedDisplay.text = "";
            LapDisplay.text = "";
            NormalInfoDisplay.text = "";

            DeathScreen.HideAll();
            SuccessScreen.HideAll();

            StateRacing.ClearHUD(this);
            StateDead.ClearHUD(this);
            StateWon.ClearHUD(this);
        }

        private void Update() {
            CurrentState.Update(this);
        }

        public void SwitchToCountdown() {
            Hits = 0;
            UpdateHeadlights(); // Reset

            CurrentState = StateCountdown;
            CurrentState.Start(this);
        }

        public void SwitchToRacing() {
            CurrentState = StateRacing;
            CurrentState.Start(this);
        }

        public void OnStartFinishTrigger() {
            CurrentState.OnStartFinishTrigger(this);
        }

        public IEnumerator ClearInfoDisplay() {
            yield return new WaitForSeconds(1.0f);

            NormalInfoDisplay.text = "";
        }

        public void SwitchToDead() {
            CurrentState = StateDead;
            CurrentState.Start(this);
        }

        public void SwitchToWon() {
            CurrentState = StateWon;
            CurrentState.Start(this);
        }

        public void OnZombieHit() {
            Hits++;
            UpdateHeadlights();
        }

        public void UpdateHeadlights() {
            if (Hits == 0) {
                LightLeft.enabled = true;
                LightLeft.intensity = 1.877862f;
                LightRight.enabled = true;
                LightRight.intensity = 1.877862f;
                return;
            }

            if (Hits < LightFallOff.Length) {
                LightLeft.intensity = LightFallOff[Hits];
                LightLeft.enabled = true;
                LightRight.intensity = LightFallOff[Hits - LightFallOff.Length];
                LightRight.enabled = true;
                return;
            }

            if (Hits < LightFallOff.Length + LightFallOff.Length) {
                LightLeft.enabled = false;
                LightRight.intensity = LightFallOff[Hits - LightFallOff.Length];
                return;
            }

            LightRight.enabled = false;
        }

        public void OnPowerUpCollected() {
            Hits = Math.Max(0, Hits - 2);
            UpdateHeadlights();
        }
    }

    public abstract class RacingState {
        public abstract void Start(RacingController controller);

        public abstract void Update(RacingController controller);

        public virtual void OnStartFinishTrigger(RacingController controller) { }

        public virtual void ClearHUD(RacingController controller) { }
    }

    public class RacingStateCountdown : RacingState {
        public int CountdownFrom = 3;
        public int CurrentCountdown;
        public float TimeCountdownStarted;

        public override void Update(RacingController controller) {
            CurrentCountdown = CountdownFrom - Mathf.FloorToInt(Time.time - TimeCountdownStarted);

            controller.InfoDisplay.text = "" + CurrentCountdown + "";

            if (CurrentCountdown <= 0) {
                controller.InfoDisplay.text = "";
                controller.SwitchToRacing();
            }
        }

        public override void Start(RacingController controller) {
            controller.InfoDisplay.text = "";
            TimeCountdownStarted = Time.time;
            CurrentCountdown = CountdownFrom;
            controller.CarUserControl.enabled = false;
        }
    }

    public class RacingStateRacing : RacingState {
        public List<RoundInformation> Rounds;
        public RoundInformation CurrentRound;
        public PausableStopwatch WarningTime;

        public override void Start(RacingController controller) {
            Rounds = new List<RoundInformation>();
            CurrentRound = new RoundInformation();
            controller.CarUserControl.enabled = true;

            ClearHUD(controller);
        }

        public void ClearHUD(RacingController controller) {
            controller.InfoDisplay.text = "";
            controller.TimeDisplay.text = "";
            controller.WarningMessageDisplay.text = "";
            controller.SpeedDisplay.text = "";
            controller.NormalInfoDisplay.text = "";
        }

        public override void Update(RacingController controller) {
            UpdateTimerPauseState(controller);
            UpdateTime(controller);
            UpdateWarnings(controller);
            UpdateSpeed(controller);
        }

        private void UpdateTimerPauseState(RacingController controller) {
            if (controller.PauseMenuManager.GamePaused) {
                if (WarningTime != null && !WarningTime.IsPaused) {
                    WarningTime.Pause();
                }

                if (CurrentRound != null && !CurrentRound.Timer.IsPaused) {
                    CurrentRound.Timer.Pause();
                }
            }
            else {
                if (WarningTime != null && WarningTime.IsPaused) {
                    WarningTime.Start();
                }

                if (CurrentRound != null && CurrentRound.Timer.IsPaused) {
                    CurrentRound.Timer.Start();
                }
            }
        }

        private void UpdateSpeed(RacingController controller) {
            controller.SpeedDisplay.text = controller.CarController.CurrentSpeed.ToString("000") + " km/h";
        }

        private void UpdateWarnings(RacingController controller) {
            if (controller.RoadController.OffTrack) {
                StartWarningIfNotRunning(controller);
                controller.WarningMessageDisplay.text = "Off Track<br>The Zombies will eat you!";
            }
            else if (controller.RoadController.WrongDirection) {
                StartWarningIfNotRunning(controller);
                controller.WarningMessageDisplay.text = "Wrong direction<br>The Zombies will devour you!";
            }
            else if (controller.CarController.CurrentSpeed < controller.MinSpeed) {
                // prevent stop
            }
            else if (controller.Hits > controller.WarningHits) {
                // prevent stop
            }
            else {
                controller.WarningMessageDisplay.text = "";
                StopWarning(controller);
            }

            if (controller.Hits > controller.WarningHits) {
                StartWarningIfNotRunning(controller);
            }

            if (controller.CarController.CurrentSpeed < controller.MinSpeed) {
                controller.SpeedDisplayBlink.StartBlink();
                StartWarningIfNotRunning(controller);
            }
            else {
                controller.SpeedDisplayBlink.StopBlink();
            }

            if (WarningTime != null && WarningTime.IsRunning) {
                var a = controller.WarningZombie;
                var volume = controller.WarningIntensity.Evaluate(WarningTime.ElapsedMilliseconds / 1000.0f / controller.SecondsTillDeath);
                a.SetVolume(volume);

                if (volume >= 1.0f) { // time elapsed = dead
                    ClearHUD(controller);
                    a.Fadeout();
                    controller.CarController.Stop();
                    controller.SwitchToDead();
                }
            }
        }


        private void StopWarning(RacingController controller) {
            if (WarningTime == null) {
                return;
            }

            WarningTime.Stop();
            var a = controller.WarningZombie;
            a.Fadeout();
        }

        private void StartWarningIfNotRunning(RacingController controller) {
            var wasRunning = true;
            if (WarningTime == null) {
                WarningTime = new PausableStopwatch();
                wasRunning = false;
            }
            else if (!WarningTime.IsRunning) {
                WarningTime.Start();
                wasRunning = false;
            }

            if (!wasRunning) {
                // play sound
                var a = controller.WarningZombie;
                a.Play(controller.WarningIntensity.Evaluate(0));
            }
        }

        private void UpdateTime(RacingController controller) {
            controller.LapDisplay.text = (Rounds.Count + 1) + " / " + controller.LapsToWin;

            var currentTime = FloatToRaceTime(CurrentRound.Duration());

            var lastTimes = "";
            for (var i = Rounds.Count - 1; i >= Math.Max(Rounds.Count - 3, 0); i--) {
                var r = Rounds[i];
                lastTimes += FloatToRaceTime(r.Duration()) + "\n";
            }

            var timeDisplay = currentTime + "\n<size=50%>";
            timeDisplay += lastTimes;
            timeDisplay += "</size>";

            controller.TimeDisplay.text = timeDisplay;
        }

        public static string FloatToRaceTime(TimeSpan time) {
            var minutes = time.Minutes; //Mathf.FloorToInt(time / 60.0f);
            var seconds = time.Seconds; //Mathf.FloorToInt(time - minutes * 60);
            var millis = time.Milliseconds; //Mathf.FloorToInt(time - seconds * 60 - minutes * 60);

            return minutes.ToString("00") + ":" + seconds.ToString("00") + "." + millis.ToString("000");
        }

        public override void OnStartFinishTrigger(RacingController controller) {
            CurrentRound.Stop();
            Rounds.Add(CurrentRound);
            var roundTime = -1;
            if (CurrentRound != null) {
                roundTime = CurrentRound.Duration().Milliseconds;
            }

            CurrentRound = new RoundInformation();

            if (Rounds.Count >= controller.LapsToWin) {
                controller.WarningZombie.Stop();
                controller.CarController.Stop();
                controller.SwitchToWon();
            }
            else {
                if (Rounds.Count == controller.LapsToWin - 1) {
                    controller.NormalInfoDisplay.text = "Final Round";
                    controller.FinalRoundAudio.Play();
                }
                else {
                    controller.NormalInfoDisplay.text = "Next Lap";

                    // is this a new best time?
                    if (roundTime > 0) {
                        for (var i = 0; i < Rounds.Count; i++) {
                            if (Rounds[i].Duration().Milliseconds < roundTime) {
                                controller.NewBestTime.Play();
                                break;
                            }
                        }
                    }
                }

                controller.StartCoroutine(controller.ClearInfoDisplay());
            }
        }
    }

    public class RacingStateDead : RacingState {
        public override void ClearHUD(RacingController controller) {
            controller.DeathScreen.HideAll();
            controller.SuccessScreen.HideAll();
        }

        public override void Start(RacingController controller) {
            controller.StateRacing.ClearHUD(controller);
            controller.DeathScreen.ShowAll();
            controller.CarUserControl.enabled = false;
            controller.ZombieEatingAudio.Play();
        }

        public override void Update(RacingController controller) { }
    }

    public class RacingStateWon : RacingState {
        public override void ClearHUD(RacingController controller) {
            controller.DeathScreen.HideAll();
            controller.SuccessScreen.HideAll();
        }

        public override void Start(RacingController controller) {
            controller.StateRacing.ClearHUD(controller);
            controller.SuccessScreen.ShowAll();
            controller.CarUserControl.enabled = false;
        }

        public override void Update(RacingController controller) { }
    }

    public class RoundInformation {
        public readonly PausableStopwatch Timer;
        public bool Stopped => !Timer.IsRunning;

        public int Penalties;

        private TimeSpan? Elapsed;

        public RoundInformation() {
            Timer = new PausableStopwatch();
        }

        public TimeSpan Duration() {
            return Timer.Elapsed;
        }

        public void Stop() {
            Elapsed = Timer.Elapsed;
            Timer.Stop();
        }
    }
}