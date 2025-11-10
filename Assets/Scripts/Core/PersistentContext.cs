using UnityEngine;

namespace A2.Core
{
    // Place on a GameObject in Boot scene with EventBus + AudioService + GameManager
    public class PersistentContext : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}