using TMPro;
using UnityEngine;

public class OSKKeyButton : MonoBehaviour
{
    public string key; // np. "A", "B", "SPACE", "BACKSPACE"
    public OnScreenKeyboard keyboard;

    public void Press()
    {
        if (keyboard != null)
        {
            keyboard.PressKey(key);
            Debug.Log("kliknieto");
        }
    }
}
