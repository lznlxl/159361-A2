using UnityEngine;

namespace A2.AI
{
    public class AISenses : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField] private float forwardProbe = 5f;
        [SerializeField] private float downProbe = 5f;
        [SerializeField] private LayerMask groundMask;

        public bool HasGround { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        public float DistanceToLip { get; private set; }

        Vector3 lastNormal;

        void FixedUpdate()
        {
            // Down
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit dhit, downProbe, groundMask))
            {
                HasGround = true;
                GroundNormal = dhit.normal;
            }
            else
            {
                HasGround = false;
                GroundNormal = Vector3.up;
            }

            // Forward "lip" detection (change in normal ahead)
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, transform.forward, out RaycastHit fhit, forwardProbe, groundMask))
            {
                float dot = Vector3.Dot(fhit.normal, lastNormal);
                if (lastNormal == Vector3.zero || dot < 0.98f)
                    DistanceToLip = fhit.distance;
                lastNormal = fhit.normal;
            }
            else
            {
                DistanceToLip = forwardProbe;
            }
        }
    }
}