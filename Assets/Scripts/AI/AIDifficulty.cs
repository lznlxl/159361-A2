using UnityEngine;

namespace A2.AI
{
    [CreateAssetMenu(menuName="A2/AI/Difficulty", fileName="AIDifficulty")]
    public class AIDifficulty : ScriptableObject
    {
        [Header("Jump timing window bias (seconds): negative = earlier, positive = later")]
        public float timingBias = 0.0f;
        [Header("Timing jitter (seconds) added to decision")]
        public float timingJitter = 0.02f;
        [Header("Tilt control strength (0..1)")]
        public float tiltStrength = 0.6f;
        [Header("Seeded randomness (0=use runtime)")]
        public int fixedSeed = 0;
    }
}