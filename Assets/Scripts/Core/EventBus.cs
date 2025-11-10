using System;
using UnityEngine;

namespace A2.Core
{
    [DefaultExecutionOrder(-500)]
    public class EventBus : MonoBehaviour
    {
        public static EventBus I { get; private set; }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        // Game flow
        public event Action<bool> Loading;
        public void RaiseLoading(bool v) => Loading?.Invoke(v);

        public event Action CountdownStarted;
        public void RaiseCountdownStarted() => CountdownStarted?.Invoke();

        public event Action<int> CountdownTick; // 3,2,1
        public void RaiseCountdownTick(int t) => CountdownTick?.Invoke(t);

        public event Action CountdownGo; // GO!
        public void RaiseCountdownGo() => CountdownGo?.Invoke();

        public event Action<RunResult> RunEnded;
        public void RaiseRunEnded(RunResult r) => RunEnded?.Invoke(r);

        // Telemetry to HUD
        public event Action<float> DistanceUpdated;
        public void RaiseDistance(float m) => DistanceUpdated?.Invoke(m);

        public event Action<float> SpeedUpdated; // m/s
        public void RaiseSpeed(float ms) => SpeedUpdated?.Invoke(ms);

        public event Action<float> AngleUpdated; // degrees
        public void RaiseAngle(float deg) => AngleUpdated?.Invoke(deg);

        public event Action<float> FlightTimeUpdated; // seconds
        public void RaiseFlight(float s) => FlightTimeUpdated?.Invoke(s);

        public event Action<LandingGrade> LandingGraded;
        public void RaiseLandingGrade(LandingGrade g) => LandingGraded?.Invoke(g);
    }

    public enum LandingGrade { Perfect, Good, Sketchy, Crash }

    [Serializable]
    public struct RunResult
    {
        public float Distance;
        public float FlightTime;
        public LandingGrade Grade;
        public int Seed;
    }
}