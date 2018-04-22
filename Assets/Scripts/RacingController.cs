using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
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
        public BlinkText SpeedDisplayBlink;
        public TextMeshProUGUI WarningMessageDisplay;

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
                LightLeft.intensity = 1.0f;
                LightRight.enabled = true;
                LightRight.intensity = 1.0f;
                return;
            }

            if (Hits < LightFallOff.Length) {
                LightLeft.intensity = LightFallOff[Hits];
                return;
            }

            if (Hits < LightFallOff.Length + LightFallOff.Length) {
                LightLeft.enabled = false;
                LightRight.intensity = LightFallOff[Hits - LightFallOff.Length];
                return;
            }

            LightRight.enabled = false;
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
        public Stopwatch WarningTime;

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
        }

        public override void Update(RacingController controller) {
            UpdateTime(controller);
            UpdateWarnings(controller);
            UpdateSpeed(controller);
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
                WarningTime = Stopwatch.StartNew();
                wasRunning = false;
            }
            else if (!WarningTime.IsRunning) {
                WarningTime.Restart();
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
            CurrentRound = new RoundInformation();

            if (Rounds.Count >= controller.LapsToWin) {
                controller.WarningZombie.Stop();
                controller.CarController.Stop();
                controller.SwitchToWon();
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
        private readonly System.Diagnostics.Stopwatch Timer;
        public bool Stopped => !Timer.IsRunning;

        public int Penalties;

        private TimeSpan? Elapsed;

        public RoundInformation() {
            Timer = new System.Diagnostics.Stopwatch();
            Timer.Start();
        }

        public TimeSpan Duration() {
            if (Elapsed.HasValue) {
                return Elapsed.Value;
            }

            return Timer.Elapsed;
        }

        public void Stop() {
            Elapsed = Timer.Elapsed;
        }
    }
}