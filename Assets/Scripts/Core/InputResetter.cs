using UnityEngine;
using UnityEngine.InputSystem;

public class InputResetter : MonoBehaviour
{
    void OnEnable()
    {
        // Re-enable all actions if the InputSystem is alive
        if (InputSystem.actions != null)
        {
            foreach (var action in InputSystem.actions)
            {
                if (!action.enabled)
                    action.Enable();
            }
        }
    }
}