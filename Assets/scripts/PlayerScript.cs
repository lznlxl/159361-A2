using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    // ====== Start Gate ======
    [Header("Start Gate")]
    public Transform gateSeat;
    public Vector3 gateLocalOffset = new Vector3(0f, 0f, 0f);
    private bool parentToSeatWhileSeated = true;

    [Header("Refs")]
    public Rigidbody rb;
    public Animator animator;
    public LayerMask groundMask;
    public LayerMask inrunMask;

    [Header("Takeoff")]
    public float takeoffBoost;
    public float maxTakeoffSpeed = 40f;

    [Header("Flight - A/D Only")]
    public float flightSteerForce = 15f;
    public float maxFlightSpeed = 35f;

    [Header("Landing & Post-landing")]
    private float preLandRay = 0.5f;
    public float passingDuration = 0.7f;             // time in Passing before Braking
    public float landingDamp = 0.3f;                 // instant velocity damp on land
    public float brakingDrag = 2.5f;                 // added drag while braking

    [Header("Ground check")]
    public float groundCheckRadius = 0.25f;
    public float groundCheckOffset = 0.1f;

    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction jumpAction;

    [Header("Release Settings")]
    public float releasePush = 6f;                   // initial force when leaving gate

    // ---- Animator parameters ----
    private const string TRIGGER_GATE       = "Gate";
    private const string TRIGGER_INRUN      = "inrun";
    private const string TRIGGER_TAKEOFF    = "Take-off";
    private const string TRIGGER_FLIGHT     = "Flight";
    private const string TRIGGER_PRELANDING = "Pre-landing";
    private const string TRIGGER_LANDING    = "Landing";
    private const string TRIGGER_PASSING    = "Passing";

    // ---- Jumper state machine ----
    public enum JumperState
    {
        Gate = 0,
        Inrun = 1,
        Takeoff = 2,
        Flight = 3,
        PreLanding = 4,
        Landing = 5,
        Passing = 6,
        Braking = 7
    }
    private JumperState currentState;
    private static readonly int PARAM_JUMPERSTATE = Animator.StringToHash("JumperState");

    private bool isPrelanding, isLanded,isBreaking;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        GetReadyAtGate();
    }

    // ---------- INPUT SETUP ----------
    void Update()
    {
        // SPACE = leave gate OR manual take-off
        if (jumpAction.WasPressedThisFrame())
        {
            if (currentState == JumperState.Gate)
                ReleaseFromGate();
            else if (currentState == JumperState.Inrun)
                TakeOff();
        }
        if (currentState == JumperState.Takeoff)
        {
            animator.CrossFade(TRIGGER_TAKEOFF, 0.1f);
            SetJumperState(JumperState.Flight);
        }
        if (currentState == JumperState.Flight)
        {
            animator.CrossFade(TRIGGER_FLIGHT, 0.1f);
            if (isPrelanding)
            {
                SetJumperState(JumperState.PreLanding);
            }
        }
        if (currentState == JumperState.PreLanding)
        {
            animator.CrossFade(TRIGGER_PRELANDING, 0.1f);
            if (isLanded)
            {
                SetJumperState(JumperState.Landing);
            }
            
        }
        if (currentState == JumperState.Landing)
        {
            animator.CrossFade(TRIGGER_LANDING, 0.1f);
            SetJumperState(JumperState.Passing);
        }
        if (currentState == JumperState.Passing)
        {
            animator.CrossFade(TRIGGER_PASSING, 0.1f);
            if(isBreaking)
            {
                SetJumperState(JumperState.Braking);
            }
        }
    }

    private void Braking()
    {
        rb.linearVelocity = Vector3.zero;
    }

    private void Passing()
    {
        // reduce horizontal speed gradually
        Vector3 v = rb.linearVelocity;
        v = Vector3.Lerp(v, Vector3.zero, Time.fixedDeltaTime * 1.5f);
        rb.linearVelocity = v;

        if (v.magnitude < 1f)
        {
            isBreaking = true;
        }
            
    }
    private void PreLanding()
    {
        // --- Raycast to detect the ground directly below ---
        Vector3 origin = rb.position;
        float checkDistance = 2f; // closer detection range than preLandRay
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, checkDistance, groundMask, QueryTriggerInteraction.Ignore))
            return;

        // --- 1. Smoothly align rotation to the slope ---
        Vector3 slopeNormal = hit.normal;
        Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized;

        if (slopeForward.sqrMagnitude > 0.0001f)
        {
            Quaternion slopeRotation = Quaternion.LookRotation(slopeForward, slopeNormal);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, slopeRotation, Time.fixedDeltaTime * 6f));
        }

        // --- 2. Reduce downward fall speed for a soft approach ---
        Vector3 v = rb.linearVelocity;
        if (v.y < -5f) // only slow when falling fast
        {
            v.y = Mathf.Lerp(v.y, -5f, Time.fixedDeltaTime * 3f); // gentle damping
            rb.linearVelocity = v;
        }

        // --- 3. Optional: apply small stabilizing lift to smooth out descent ---
        rb.AddForce(Vector3.up * 1.5f, ForceMode.Acceleration);

        // --- 4. Check if actually touching ground yet ---
        if (hit.distance <= 0.3f)
        {
            isLanded = true;
        }
    }

    private void CheckForPreLanding()
    {
        // Raycast downward from the jumper to check distance to ground
        Vector3 origin = rb.position;
        float checkDistance = preLandRay;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, checkDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            // Optional: debug visual
            Debug.Log("[PreLanding] Ground detected below jumper.");

            // If the jumper is close enough, prepare for landing
            isPrelanding = true;

            // Smoothly align to slope before touchdown
            Vector3 slopeNormal = hit.normal;
            Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized;

            if (slopeForward.sqrMagnitude > 0.0001f)
            {
                Quaternion slopeRotation = Quaternion.LookRotation(slopeForward, slopeNormal);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, slopeRotation, Time.fixedDeltaTime * 6f));
            }
        }
    }


    private void ApplyFlightSteering()
    {
        // --- Input ---
        Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        float horizontal = input.x;
        float pitchInput = input.y;   // W / S (optional – nose control)

        // --- Base settings ---
        float steerForce = flightSteerForce;
        float pitchTorque = 20f;               // how quickly nose tilts up/down
        float maxSpeed = maxFlightSpeed;       // from your serialized field

        // --- 1. Side (A/D) steering ---
        if (Mathf.Abs(horizontal) > 0.05f)
        {
            rb.AddForce(transform.right * horizontal * steerForce, ForceMode.Acceleration);
        }

        // --- 2. Gentle pitch control (optional W/S) ---
        if (Mathf.Abs(pitchInput) > 0.05f)
        {
            // negative pitchInput lifts the nose up slightly
            Quaternion deltaPitch = Quaternion.Euler(-pitchInput * pitchTorque * Time.fixedDeltaTime, 0f, 0f);
            rb.MoveRotation(rb.rotation * deltaPitch);
        }

        // --- 3. Speed clamp ---
        Vector3 v = rb.linearVelocity;
        float speed = v.magnitude;
        if (speed > maxSpeed)
        {
            rb.linearVelocity = v.normalized * maxSpeed;
        }

        // --- 4. Optional stability smoothing (keeps upright roll level) ---
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude > 0.001f)
        {
            Quaternion uprightRot = Quaternion.LookRotation(flatForward, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, uprightRot, Time.fixedDeltaTime * 1.5f));
        }
    }

    
    private void Flight()
    {
        // --- Adjustable settings ---
        float gravityScale = 0.2f;     // 1.0 = normal gravity, <1 = lighter fall, >1 = heavier
        float glideStability = 2.0f;   // how strongly to keep level orientation

        // --- Effective reduced gravity ---
        Vector3 reducedGravity = Physics.gravity * gravityScale;
        rb.AddForce(reducedGravity - Physics.gravity, ForceMode.Acceleration);
        // (the subtraction cancels out Unity’s built-in gravity already applied)

        // --- Optional: add a small upward "lift" when moving forward ---
        Vector3 forward = transform.forward;
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, forward);
        if (forwardSpeed > 1f)
        {
            // lift proportional to speed → more speed = more lift
            float lift = Mathf.Clamp(forwardSpeed * 0.1f, 0f, takeoffBoost);
            rb.AddForce(Vector3.up * lift, ForceMode.Acceleration);
        }

        // --- Optional: keep jumper gently level in air ---
        Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * glideStability));
    }


    private void TakeOff()
    {
        if (animator)
        {
            animator.applyRootMotion = false;
            animator.CrossFade(TRIGGER_INRUN, 0.1f);
        }
        SetJumperState(JumperState.Takeoff);

        rb.isKinematic = false;
        rb.useGravity   = true;
        
        // ---------- PRESERVE FORWARD SPEED ----------
        Vector3 rampForward = GetRampForward();                     // direction of the ramp under you
        float forwardSpeed  = Vector3.Dot(rb.linearVelocity, rampForward);
        float clampedSpeed  = Mathf.Clamp(forwardSpeed, 0f, maxTakeoffSpeed);
        Vector3 forwardVel  = rampForward * clampedSpeed;

        // ---------- VERTICAL BOOST (along ramp normal) ----------
        Vector3 rampNormal = Vector3.up;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f, groundMask))
            rampNormal = hit.normal;

        Vector3 boost = rampNormal * takeoffBoost;

        // ---------- APPLY ----------
        rb.linearVelocity = forwardVel;                 // keep the speed you earned
        rb.AddForce(boost, ForceMode.VelocityChange);   // add the pop

    }
    
    // ---------- RELEASE FROM GATE ----------
    public void ReleaseFromGate()
    {
        // Play animation
        if (animator)
        {
            animator.applyRootMotion = false;
            animator.CrossFade(TRIGGER_GATE, 0.1f);
        }
        SetJumperState(JumperState.Inrun);


        if (parentToSeatWhileSeated && transform.parent == gateSeat)
        {
            transform.SetParent(null, worldPositionStays: true);
            parentToSeatWhileSeated = false;
        }
            

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 forward = GetRampForward();

        // Apply initial push
        if (releasePush > 0f)
            rb.AddForce(forward * releasePush, ForceMode.VelocityChange);


    }

    // ---------- Get Ready At Gate ----------
    public void GetReadyAtGate()
    {
        
        // Zero motion
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        SetJumperState(JumperState.Gate);
        // Animator state
        if (animator)
        {
            animator.applyRootMotion = false;
            animator.Play(TRIGGER_GATE);
        }
        

        if (parentToSeatWhileSeated)
        {
            // Parent so we follow the seat exactly
            transform.SetParent(gateSeat, worldPositionStays: true);
            transform.localPosition = gateLocalOffset;
            transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }

        // Freeze physics while seated
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    
    // ---------- INRUN SPEED-UP ----------
    void FixedUpdate()
    {
        switch (currentState)
        {
            case JumperState.Inrun:
                AlignWithSlope(inrunMask);
                break;

            case JumperState.Flight:
                Flight();
                ApplyFlightSteering();
                CheckForPreLanding();
                break;

            case JumperState.PreLanding:
                PreLanding();
                break;

            case JumperState.Passing:
                Passing();
                break;
            // case JumperState.Braking:
            //     Braking();
            //     break;
        }
        
    }
    private Vector3 GetRampForward()
    {
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2.0f, inrunMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 normal = hit.normal;
            Vector3 f = gateSeat ? gateSeat.forward : transform.forward;
            Vector3 tangent = Vector3.ProjectOnPlane(f, normal).normalized;
            if (tangent.sqrMagnitude > 0.0001f) return tangent;
        }

        if (gateSeat) return gateSeat.forward.normalized;
        return transform.forward.normalized;
    }

    // ---------- STATE ----------
    public void SetJumperState(JumperState newState)
    {
        currentState = newState;
        if (animator)
            animator.SetInteger(PARAM_JUMPERSTATE, (int)newState);
    }

    private void AlignWithSlope(LayerMask mask)
    {
        // Cast downward from the jumper to find the slope below
        Vector3 origin = transform.position + Vector3.up * 0.2f; // start slightly above feet
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2.0f, mask, QueryTriggerInteraction.Ignore))
        {
            // Surface normal & forward direction projected onto slope
            Vector3 slopeNormal = hit.normal;
            Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized;

            if (slopeForward.sqrMagnitude > 0.0001f)
            {
                // Smooth orientation to match slope
                Quaternion slopeRotation = Quaternion.LookRotation(slopeForward, slopeNormal);
                // Add 10° upward pitch (negative X tilts nose up)
                Quaternion offset = Quaternion.Euler(10f, 0f, 0f);

                // Apply the offset *after* slopeRotation
                Quaternion finalRot = slopeRotation * offset;
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, finalRot, Time.fixedDeltaTime * 8f));
            }

            //  Adjust vertical position so skis sit right on slope
            float skinOffset = 0.07f; // small gap to avoid z-fighting
            Vector3 targetPosition = hit.point + slopeNormal * skinOffset;

            // Smoothly move to the target contact point
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * 16f));
        }
    }

}
