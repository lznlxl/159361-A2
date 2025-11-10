using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace A2.Core
{
    public class GameManager : MonoBehaviour
    {
        public enum GameState { Boot, MainMenu, Loading, Countdown, Playing, Paused, RunEnded, Results }
        public static GameManager I { get; private set; }
        public StateMachine<GameState> SM { get; private set; }

        [SerializeField] private float countdownSeconds = 3f;
        private RunResult lastRun;

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);
            SM = new StateMachine<GameState>(GameState.Boot);
        }

        void Start()
        {
            StartCoroutine(BootFlow());
        }

        IEnumerator BootFlow()
        {
            EventBus.I.RaiseLoading(true);
            yield return LoadSceneAsync(SceneNames.MainMenu);
            EventBus.I.RaiseLoading(false);
            SM.SetState(GameState.MainMenu);
        }

        public void RequestStartGame()
        {
            StartCoroutine(LoadGameplay());
        }

        IEnumerator LoadGameplay()
        {
            SM.SetState(GameState.Loading);
            EventBus.I.RaiseLoading(true);
            yield return LoadSceneAsync(SceneNames.Game);
            EventBus.I.RaiseLoading(false);
            StartCoroutine(CountdownThenPlay());
        }

        IEnumerator CountdownThenPlay()
        {
            SM.SetState(GameState.Countdown);
            EventBus.I.RaiseCountdownStarted();
            for (int t = Mathf.CeilToInt(countdownSeconds); t > 0; t--)
            {
                EventBus.I.RaiseCountdownTick(t);
                yield return new WaitForSecondsRealtime(1f);
            }
            EventBus.I.RaiseCountdownGo();
            SM.SetState(GameState.Playing);
        }

        public void Pause(bool v)
        {
            if (v) { Time.timeScale = 0f; SM.SetState(GameState.Paused); }
            else { Time.timeScale = 1f; SM.SetState(GameState.Playing); }
        }

        public void OnRunEnded(RunResult r)
        {
            if (SM.State == GameState.RunEnded || SM.State == GameState.Results) return;
            lastRun = r;
            SM.SetState(GameState.RunEnded);
            StartCoroutine(ShowResults());
        }

        IEnumerator ShowResults()
        {
            yield return LoadSceneAsync(SceneNames.Results);
            SM.SetState(GameState.Results);
            EventBus.I.RaiseRunEnded(lastRun);
        }

        public void RestartRun()
        {
            StartCoroutine(LoadGameplay());
        }

        public void GoToMenu()
        {
            StartCoroutine(LoadMenu());
        }

        IEnumerator LoadMenu()
        {
            Time.timeScale = 1f;
            yield return LoadSceneAsync(SceneNames.MainMenu);
            SM.SetState(GameState.MainMenu);
        }

        static IEnumerator LoadSceneAsync(string scene)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene)) yield break;
            AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
        }
    }
}