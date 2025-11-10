using UnityEngine;
using UnityEngine.UIElements;
using A2.Core;

namespace A2.UI
{
    public class UI_MainMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument doc;
        [SerializeField] private string startButtonName = "StartButton";
        [SerializeField] private string quitButtonName = "QuitButton";
        [SerializeField] private string optionsSliderName = "MasterVolume";

        Button startBtn, quitBtn;
        Slider volumeSlider;

        void Awake()
        {
            if (doc == null) doc = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            var root = doc.rootVisualElement;

            startBtn = root.Q<Button>(startButtonName);
            quitBtn = root.Q<Button>(quitButtonName);
            volumeSlider = root.Q<Slider>(optionsSliderName);

            if (startBtn != null) startBtn.clicked += OnStart;
            if (quitBtn != null) quitBtn.clicked += OnQuit;
            if (volumeSlider != null) volumeSlider.RegisterValueChangedCallback(OnVolumeChanged);
        }

        void OnDisable()
        {
            if (startBtn != null) startBtn.clicked -= OnStart;
            if (quitBtn != null) quitBtn.clicked -= OnQuit;
            if (volumeSlider != null) volumeSlider.UnregisterValueChangedCallback(OnVolumeChanged);
        }

        void OnStart() => GameManager.I.RequestStartGame();

        void OnVolumeChanged(ChangeEvent<float> evt)
        {
            var svc = Object.FindFirstObjectByType<AudioService>();
            if (svc != null) svc.SetMasterVolume(evt.newValue);
        }

        void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}