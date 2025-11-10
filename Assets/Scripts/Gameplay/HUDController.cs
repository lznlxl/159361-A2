using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls simple HUD elements: score readouts and key prompts.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Score Labels")]
    [SerializeField] private TextMeshProUGUI playerScoreLabel;
    [SerializeField] private string playerScoreFormat = "Score: {0}";

    [Header("Key Prompts")]
    [SerializeField] private CanvasGroup spacePrompt;
    [SerializeField] private CanvasGroup adPrompt;
    [SerializeField] private CanvasGroup escPrompt;
    [SerializeField] private CanvasGroup enterPrompt;

    [Header("Prompt Visuals")]
    [SerializeField] private float idleAlpha = 0.45f;
    [SerializeField] private float pressedAlpha = 1f;
    [SerializeField] private float fadeSpeed = 10f;

    [Header("Scenes")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string gameplaySceneName = "GameScene";

    void Update()
    {
        UpdateScoreLabels();
        UpdateKeyPrompts();
        HandleHotkeys();
    }

    private void UpdateScoreLabels()
    {
        if (playerScoreLabel)
            playerScoreLabel.text = string.Format(playerScoreFormat, ScoreSystem.playerScore);
    }

    private void UpdateKeyPrompts()
    {
        if (Keyboard.current == null)
            return;

        SetPromptAlpha(spacePrompt, Keyboard.current.spaceKey.isPressed);
        bool adPressed = (Keyboard.current.aKey?.isPressed ?? false) || (Keyboard.current.dKey?.isPressed ?? false);
        SetPromptAlpha(adPrompt, adPressed);
        SetPromptAlpha(escPrompt, Keyboard.current.escapeKey.isPressed);
        bool enterPressed = (Keyboard.current.enterKey?.isPressed ?? false) || (Keyboard.current.numpadEnterKey?.isPressed ?? false);
        SetPromptAlpha(enterPrompt, enterPressed);
    }

    private void HandleHotkeys()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame && !string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
        }

        if ((Keyboard.current.enterKey?.wasPressedThisFrame ?? false
            || Keyboard.current.numpadEnterKey?.wasPressedThisFrame == true)
            && !string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
    }

    private void SetPromptAlpha(CanvasGroup group, bool pressed)
    {
        if (!group)
            return;

        float target = pressed ? pressedAlpha : idleAlpha;
        group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.deltaTime);
    }
}
