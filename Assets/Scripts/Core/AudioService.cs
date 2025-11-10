using UnityEngine;
using UnityEngine.Audio;

namespace A2.Core
{
    public class AudioService : MonoBehaviour
    {
        public static AudioService I { get; private set; } = null!;
        [SerializeField] private AudioMixer masterMixer = null!; // expose group named "Master"
        void Awake(){ if (I!=null && I!=this){ Destroy(gameObject); return; } I=this; DontDestroyOnLoad(gameObject);}        
        public void SetMasterVolume(float linear01)
        {
            // Convert [0..1] to mixer dB (-80..0)
            float dB = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(linear01));
            masterMixer.SetFloat("MasterVolume", dB);
        }
    }
}