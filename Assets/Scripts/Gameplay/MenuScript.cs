using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private TextMeshProUGUI instruction;
    [SerializeField] private bool hideInstructionsOnStart = true;
    
    public void PlayButton() {
        // Load Play Scene
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void HowToPlayButton() {
        if (!instruction)
        {
            Debug.LogWarning("Instructions panel not assigned.");
            return;
        }

        bool shouldShow = !instruction.gameObject.activeSelf;
        instruction.gameObject.SetActive(shouldShow);

        if (shouldShow)
            Debug.Log("Showing instructions.");
    }

    public void ExitButton() {
        Application.Quit();
    }

    void Start()
    {
        if (instruction && hideInstructionsOnStart)
            instruction.gameObject.SetActive(false);
    }

    void Update() {}
}
