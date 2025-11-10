using UnityEngine;
using System.Collections;

public class GameRoundManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int maxAttempts = 3;

    [Header("References")]
    public PlayerScript player;
    public PlayerLandingScorer playerScorer;

    [Header("Camera")]
    public Behaviour playerCameraRig;

    [Header("Turn Timing")]
    public float betweenAttemptsDelay = 2f;
    public float brakingWaitTimeout = 8f;

    private int attemptCount = 0;
    private Coroutine turnFlow;
    private bool isGameOver = false;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Missing player references on GameRoundManager.");
            enabled = false;
            return;
        }

        ResetScores();
        attemptCount = 0;
        SetActiveCamera(true);
        isGameOver = false;

        turnFlow = StartCoroutine(PrepareActiveJumper());
    }

    public void ScoreAttempt(int points)
    {
        if (isGameOver)
            return;

        if (turnFlow != null)
        {
            StopCoroutine(turnFlow);
            turnFlow = null;
        }

        turnFlow = StartCoroutine(HandlePostScore());
    }

    private IEnumerator HandlePostScore()
    {
        yield return WaitForBrakingStop(player);
        EnablePlayerInput(false);

        attemptCount = Mathf.Clamp(attemptCount + 1, 0, maxAttempts);
        bool shouldEnd = attemptCount >= maxAttempts;
        if (shouldEnd)
        {
            EndGame();
            turnFlow = null;
            yield break;
        }

        yield return new WaitForSeconds(betweenAttemptsDelay);
        yield return PrepareActiveJumper();
    }

    private IEnumerator PrepareActiveJumper()
    {
        yield return null;

        if (player == null)
        {
            Debug.LogError("Active jumper reference missing.");
            yield break;
        }

        playerScorer?.ResetAttempt();
        EnablePlayerInput(true);

        SetActiveCamera(true);
        player.GetReadyAtGate();

        Debug.Log($"Player Attempt #{attemptCount + 1}");
        turnFlow = null;
    }

    private IEnumerator WaitForBrakingStop(PlayerScript jumper)
    {
        if (jumper == null || jumper.rb == null)
            yield break;

        float timer = 0f;
        while (timer < brakingWaitTimeout)
        {
            bool brakingState = jumper.State == PlayerScript.JumperState.Braking;
            bool slowEnough = jumper.rb.linearVelocity.magnitude <= 0.1f;

            if (brakingState && slowEnough)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning($"Timeout while waiting for {jumper.name} to finish braking. Forcing reset.");
    }

    private void EnablePlayerInput(bool enable)
    {
        if (player == null)
            return;

        if (enable)
        {
            player.moveAction?.Enable();
            player.jumpAction?.Enable();
        }
        else
        {
            player.moveAction?.Disable();
            player.jumpAction?.Disable();
        }
    }

    private void SetActiveCamera(bool showPlayer)
    {
        if (playerCameraRig != null)
            playerCameraRig.enabled = showPlayer;
    }

    private void ResetScores()
    {
        ScoreSystem.playerScore = 0;
    }

    private void EndGame()
    {
        if (turnFlow != null)
        {
            StopCoroutine(turnFlow);
            turnFlow = null;
        }

        EnablePlayerInput(false);
        SetActiveCamera(true);
        isGameOver = true;

        Debug.Log("Game Over!");
        Debug.Log($"Player Score: {ScoreSystem.playerScore}");
    }
}
