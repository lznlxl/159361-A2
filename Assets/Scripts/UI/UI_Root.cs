using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace A2.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UI_Root : MonoBehaviour
    {
        [SerializeField] private UIDocument doc;
        private VisualElement root;

        public UIDocument Doc => doc;
        public VisualElement Root => root;

        void Awake()
        {
            if (doc == null)
                doc = GetComponent<UIDocument>();

            root = doc.rootVisualElement;
            AttachThemeIfMissing();
        }

        private void AttachThemeIfMissing()
        {
            if (root == null)
                return;

            bool hasTheme = false;
            int count = root.styleSheets.count;

            for (int i = 0; i < count; i++)
            {
                var sheet = root.styleSheets[i];
                if (sheet != null && sheet.name.ToLower().Contains("theme"))
                {
                    hasTheme = true;
                    break;
                }
            }

            if (!hasTheme)
            {
#if UNITY_EDITOR
                var theme = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/Theme.uss");
                if (theme != null)
                {
                    root.styleSheets.Add(theme);
                    Debug.Log("[UI_Root] Theme.uss attached via AssetDatabase.");
                }
                else
                {
                    Debug.LogWarning("[UI_Root] Theme.uss not found at Assets/UI/USS/Theme.uss");
                }
#else
                Debug.LogWarning("[UI_Root] Theme.uss missing (must be linked manually in build).");
#endif
            }
        }

        public void SetVisible(bool visible)
        {
            if (root == null)
                root = doc.rootVisualElement;

            root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Toggle()
        {
            if (root == null)
                root = doc.rootVisualElement;

            bool isVisible = root.style.display == DisplayStyle.Flex;
            root.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}