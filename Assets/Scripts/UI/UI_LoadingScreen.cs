using UnityEngine;
using UnityEngine.UIElements;
using A2.Core;

namespace A2.UI
{
    public class UI_LoadingScreen : MonoBehaviour
    {
        [SerializeField] private UIDocument doc;

        void Awake()
        {
            if (doc == null) doc = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            EventBus.I.Loading += OnLoading;
            OnLoading(false);
        }

        void OnDisable()
        {
            if (EventBus.I != null) EventBus.I.Loading -= OnLoading;
        }

        void OnLoading(bool v)
        {
            if (doc != null && doc.rootVisualElement != null)
                doc.rootVisualElement.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}