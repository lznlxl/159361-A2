using UnityEngine;

[RequireComponent(typeof(PlayerScript))]
public class PlayerLandingScorer : MonoBehaviour
{
    public LayerMask landingMask;
    public float stopSpeed = 0.25f;
    public float holdTime = 0.6f;
    public float checkRadius = 0.6f;
    public Vector3 checkOffset = new Vector3(0, 0.1f, 0);
    public GameRoundManager game;

    private PlayerScript player;
    private Rigidbody rb;
    private float stoppedTimer = 0f;
    private LandingCircle circleOccupied = null;
    private bool attemptScored = false;

    void Awake()
    {
        player = GetComponent<PlayerScript>();
        rb = player.rb;
    }

    void FixedUpdate()
    {
        if (game == null || attemptScored) return;

        bool postLanding = player.State == PlayerScript.JumperState.Passing
                        || player.State == PlayerScript.JumperState.Braking
                        || player.State == PlayerScript.JumperState.Landing;

        if (!postLanding)
        {
            stoppedTimer = 0f;
            circleOccupied = null;
            return;
        }

        bool insideCircle = TryGetBestCircle(out var bestCircle, out int bestPoints);
        if (!insideCircle)
        {
            circleOccupied = null;
            stoppedTimer = 0f;
            return;
        }

        bool slowEnough = rb.linearVelocity.sqrMagnitude <= stopSpeed * stopSpeed;
        if (slowEnough && bestCircle == circleOccupied)
        {
            stoppedTimer += Time.fixedDeltaTime;
        }
        else
        {
            circleOccupied = bestCircle;
            stoppedTimer = 0f;
        }

        if (stoppedTimer >= holdTime)
        {
            attemptScored = true;
            ScoreSystem.AddPlayerPoints(bestPoints);
            game.ScoreAttempt(bestPoints);
        }
    }

    private bool TryGetBestCircle(out LandingCircle circle, out int points)
    {
        circle = null;
        points = 0;

        Vector3 center = transform.position - checkOffset;
        Collider[] hits = Physics.OverlapSphere(center, checkRadius, landingMask, QueryTriggerInteraction.Collide);

        foreach (var c in hits)
        {
            if (c.TryGetComponent<LandingCircle>(out var lc) && lc.points >= points)
            {
                points = lc.points;
                circle = lc;
            }
        }

        return circle != null;
    }

    public void ResetAttempt()
    {
        attemptScored = false;
        stoppedTimer = 0f;
        circleOccupied = null;
    }
}
