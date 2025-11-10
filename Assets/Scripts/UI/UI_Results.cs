using UnityEngine;
using UnityEngine.UIElements;
using A2.Core;

namespace A2.UI
{
    public class UI_Results : MonoBehaviour
    {
        [SerializeField] private UIDocument doc;
        Label dist, time, grade;
        Button retryBtn, menuBtn;

        void Awake(){ if (doc == null) doc = GetComponent<UIDocument>(); }

        void OnEnable()
        {
            var r = doc.rootVisualElement;
            dist = r.Q<Label>("ResultDistance");
            time = r.Q<Label>("ResultTime");
            grade = r.Q<Label>("ResultGrade");
            retryBtn = r.Q<Button>("RetryButton");
            menuBtn = r.Q<Button>("MenuButton");

            EventBus.I.RunEnded += OnRun;

            if (retryBtn != null) retryBtn.clicked += OnRetry;
            if (menuBtn != null) menuBtn.clicked += OnMenu;
        }

        void OnDisable()
        {
            if (EventBus.I != null) EventBus.I.RunEnded -= OnRun;
            if (retryBtn != null) retryBtn.clicked -= OnRetry;
            if (menuBtn != null) menuBtn.clicked -= OnMenu;
        }

        void OnRun(RunResult r)
        {
            if (dist != null) dist.text = r.Distance.ToString("0.00") + " m";
            if (time != null) time.text = r.FlightTime.ToString("0.00") + " s";
            if (grade != null) grade.text = r.Grade.ToString().ToUpperInvariant();
        }

        void OnRetry()=> GameManager.I.RestartRun();
        void OnMenu()=> GameManager.I.GoToMenu();
    }
}