using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LandingCircle : MonoBehaviour
{
    [Tooltip("Points for stopping inside this circle. Smaller circle = higher points.")]
    public int points = 10;

    void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;  // IMPORTANT
    }
}
