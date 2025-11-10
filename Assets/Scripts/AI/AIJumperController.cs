using UnityEngine;
using A2.Core;

namespace A2.AI
{
    [RequireComponent(typeof(AISenses))]
    [RequireComponent(typeof(Rigidbody))]
    public class AIJumperController : MonoBehaviour
    {
        public enum AIState { Idle, Rolling, TakeoffPrep, Jumping, Flying, Landing, Finished }

        [Header("Difficulty")]
        public AIDifficulty difficulty;

        // Hooks to Player-like API (wire these from PlayerScript or GameManager)
        public System.Func<bool> CanJump;
        public System.Action JumpAction;
        public System.Action<float> AirTilt;
        public System.Func<bool> IsGrounded;

        AISenses senses;
        Rigidbody rb;
        AIState state = AIState.Idle;
        bool hasJumped;
        float flightTimer;

        void Awake()
        {
            senses = GetComponent<AISenses>();
            rb = GetComponent<Rigidbody>();
        }

        void OnEnable()
        {
            state = AIState.Rolling;
            hasJumped = false;
            flightTimer = 0f;
        }

        void FixedUpdate()
        {
            switch (state)
            {
                case AIState.Rolling:
                    // Look for takeoff lip ahead and schedule a jump slightly before it
                    if (senses.DistanceToLip < 1.2f)
                        TryJump();
                    break;

                case AIState.Jumping:
                    // small delay then switch to flying
                    break;

                case AIState.Flying:
                    flightTimer += Time.fixedDeltaTime;
                    // gentle auto-level
                    AirTilt?.Invoke(0f);
                    if (IsGrounded != null && IsGrounded())
                        state = AIState.Landing;
                    break;

                case AIState.Landing:
                    state = AIState.Finished;
                    EventBus.I.RaiseLandingGrade(LandingGrade.Good);
                    EventBus.I.RaiseRunEnded(new RunResult { Distance = 0f, FlightTime = flightTimer, Grade = LandingGrade.Good, Seed = 0 });
                    break;
            }
        }

        void TryJump()
        {
            if (hasJumped) return;
            if (CanJump == null || CanJump())
            {
                JumpAction?.Invoke();
                hasJumped = true;
                state = AIState.Jumping;
                Invoke(nameof(EnterFlying), 0.1f);
            }
        }

        void EnterFlying() { state = AIState.Flying; }
    }
}