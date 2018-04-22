using System;
using System.Collections.Generic;
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
        
        public CarUserControl CarUserControl;
        public TextMeshProUGUI InfoDisplay;
        public TextMeshProUGUI TimeDisplay;
        public TextMeshProUGUI WarningMessageDisplay;

        public int Hits;
        public Light LightLeft;
        public Light LightRight;
        public float[] LightFallOff;

        private void Start() {
            CurrentState = StateCountdown = new RacingStateCountdown();
            CurrentState.Start(this);

            StateRacing = new RacingStateRacing();

            InfoDisplay.text = "";
            TimeDisplay.text = "";
            WarningMessageDisplay.text = "";
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

        public override void Start(RacingController controller) {
            Rounds = new List<RoundInformation>();
            CurrentRound = new RoundInformation();
            controller.CarUserControl.enabled = true;
            
            controller.WarningMessageDisplay.text = "";
            controller.InfoDisplay.text = "";
            controller.TimeDisplay.text = "";
        }

        public override void Update(RacingController controller) {
            UpdateTime(controller);

            UpdateWarnings(controller);
        }

        private static void UpdateWarnings(RacingController controller) {
            if (controller.RoadController.OffTrack) {
                controller.WarningMessageDisplay.text = "Off Track, the Zombies will get you!";
            }
            else if (controller.RoadController.WrongDirection) {
                controller.WarningMessageDisplay.text = "Wrong direction, the Zombies will get you!";
            }
            else {
                controller.WarningMessageDisplay.text = "";
            }
        }

        private void UpdateTime(RacingController controller) {
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
        }
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