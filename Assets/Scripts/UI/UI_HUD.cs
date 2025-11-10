using UnityEngine;
using UnityEngine.UIElements;
using A2.Core;

namespace A2.UI
{
    public class UI_HUD : MonoBehaviour
    {
        [SerializeField] private UIDocument doc;

        Label score, speed, angle, flight, banner;
        Label countdown;
        float displayedScore;

        void Awake()
        {
            if (doc == null) doc = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            var r = doc.rootVisualElement;
            score = r.Q<Label>("ScoreLabel");
            speed = r.Q<Label>("SpeedLabel");
            angle = r.Q<Label>("AngleLabel");
            flight = r.Q<Label>("FlightLabel");
            banner = r.Q<Label>("LandingBanner");
            countdown = r.Q<Label>("Countdown");

            EventBus.I.DistanceUpdated += OnDistance;
            EventBus.I.SpeedUpdated += OnSpeed;
            EventBus.I.AngleUpdated += OnAngle;
            EventBus.I.FlightTimeUpdated += OnFlight;
            EventBus.I.CountdownStarted += OnCountdownStart;
            EventBus.I.CountdownTick += OnCountdownTick;
            EventBus.I.CountdownGo += OnCountdownGo;
            EventBus.I.LandingGraded += OnLanding;
        }

        void OnDisable()
        {
            if (EventBus.I == null) return;
            EventBus.I.DistanceUpdated -= OnDistance;
            EventBus.I.SpeedUpdated -= OnSpeed;
            EventBus.I.AngleUpdated -= OnAngle;
            EventBus.I.FlightTimeUpdated -= OnFlight;
            EventBus.I.CountdownStarted -= OnCountdownStart;
            EventBus.I.CountdownTick -= OnCountdownTick;
            EventBus.I.CountdownGo -= OnCountdownGo;
            EventBus.I.LandingGraded -= OnLanding;
        }

        void OnDistance(float m)
        {
            // simple smoothing
            displayedScore = Mathf.Lerp(displayedScore, m, 0.2f);
            if (score != null) score.text = displayedScore.ToString("0.00") + " m";
        }

        void OnSpeed(float ms)
        {
            float kmh = ms * 3.6f;
            if (speed != null) speed.text = kmh.ToString("0.0") + " km/h";
        }

        void OnAngle(float deg)
        {
            if (angle != null) angle.text = Mathf.RoundToInt(deg) + "°";
        }

        void OnFlight(float s)
        {
            if (flight != null) flight.text = s.ToString("0.00") + " s";
        }

        void OnCountdownStart() { if (countdown != null) countdown.text = ""; }
        void OnCountdownTick(int t) { if (countdown != null) countdown.text = t.ToString(); }
        void OnCountdownGo() { if (countdown != null) countdown.text = "GO!"; Invoke(nameof(ClearCountdown), 0.5f); }
        void ClearCountdown() { if (countdown != null) countdown.text = ""; }

        void OnLanding(LandingGrade g)
        {
            if (banner == null) return;
            banner.text = g switch
            {
                LandingGrade.Perfect => "PERFECT LANDING",
                LandingGrade.Good => "GOOD LANDING",
                LandingGrade.Sketchy => "SKETCHY",
                _ => "CRASH"
            };
            banner.AddToClassList("show");
            CancelInvoke();
            Invoke(nameof(HideBanner), 2f);
        }

        void HideBanner() { if (banner != null) banner.RemoveFromClassList("show"); }
    }
}