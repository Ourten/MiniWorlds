using GameCode.Scripts.Gui;
using GameCode.Scripts.Inputs;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private void Start()
    {
        InputService.Singleton.RegisterKey("Minimap", GuiReference.MAP);
        InputService.Singleton.RegisterKey("Cancel", GuiReference.INGAME);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) &&
            !GuiManager.Instance.IsAnyGuiOpen() &&
            !InputService.Singleton.IsCursorLocked)
            InputService.Singleton.IsCursorLocked = true;

        // In the editor the cursor is ALWAYS unlocked by escape and we can't override that useless behavior
        if (Input.GetButtonDown("Cancel") && GuiManager.Instance.IsAnyGuiOpen())
            GuiManager.Instance.GetOpenedGuis().ForEach(gui => GuiManager.Instance.CloseGui(gui));
        else
        {
            foreach (var key in InputService.Singleton.GetKeys())
            {
                if (Input.GetButtonDown(key))
                    InputService.Singleton.GetAction(key).Invoke();
            }
        }
    }
}