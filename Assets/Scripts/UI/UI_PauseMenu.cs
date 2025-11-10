using UnityEngine;
using UnityEngine.UIElements;
using A2.Core;

namespace A2.UI
{
    public class UI_PauseMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument doc;
        private VisualElement root;
        private Button resumeBtn, restartBtn, menuBtn;
        private bool isVisible;

        void Awake()
        {
            if (doc == null)
                doc = GetComponent<UIDocument>();

            root = doc.rootVisualElement;
            if (root != null)
                root.style.display = DisplayStyle.None; // Start hidden
        }

        void OnEnable()
        {
            if (doc == null || doc.rootVisualElement == null)
                return;

            root = doc.rootVisualElement;

            resumeBtn = root.Q<Button>("ResumeButton");
            restartBtn = root.Q<Button>("RestartButton");
            menuBtn = root.Q<Button>("MenuButton");

            if (resumeBtn != null) resumeBtn.clicked += OnResume;
            if (restartBtn != null) restartBtn.clicked += OnRestart;
            if (menuBtn != null) menuBtn.clicked += OnMenu;
        }

        void OnDisable()
        {
            if (resumeBtn != null) resumeBtn.clicked -= OnResume;
            if (restartBtn != null) restartBtn.clicked -= OnRestart;
            if (menuBtn != null) menuBtn.clicked -= OnMenu;
        }

        void Update()
        {
            // Only toggle pause if the GameManager exists and we're in a state that can pause
            if (GameManager.I == null)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameManager.I.SM.State == GameManager.GameState.Playing)
                {
                    ShowPauseMenu(true);
                    GameManager.I.Pause(true);
                }
                else if (GameManager.I.SM.State == GameManager.GameState.Paused)
                {
                    ShowPauseMenu(false);
                    GameManager.I.Pause(false);
                }
            }
        }

        void OnResume()
        {
            ShowPauseMenu(false);
            GameManager.I.Pause(false);
        }

        void OnRestart()
        {
            ShowPauseMenu(false);
            GameManager.I.RestartRun();
        }

        void OnMenu()
        {
            ShowPauseMenu(false);
            GameManager.I.GoToMenu();
        }

        /// Toggles menu visibility in UI Toolkit.
        public void ShowPauseMenu(bool show)
        {
            if (root == null)
                root = doc.rootVisualElement;

            root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            isVisible = show;
        }
    }
}