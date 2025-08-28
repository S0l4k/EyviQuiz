using TMPro;
using UnityEngine;

public class OnScreenKeyboard : MonoBehaviour
{
    public TMP_InputField currentTarget;

    public void SetTarget(TMP_InputField target)
    {
        currentTarget = target;
        Debug.Log($"[Keyboard] Target ustawiony: {currentTarget.name}");
    }

    public void PressKey(string key)
    {
        if (currentTarget == null)
        {
            Debug.LogWarning("[Keyboard] Brak aktywnego targetu!");
            return;
        }

        if (key == "BACKSPACE")
        {
            if (currentTarget.text.Length > 0)
                currentTarget.text = currentTarget.text.Substring(0, currentTarget.text.Length - 1);
        }
        else if (key == "SPACE")
        {
            currentTarget.text += " ";
        }
        else
        {
            currentTarget.text += key;
        }

        currentTarget.caretPosition = currentTarget.text.Length;
        currentTarget.ForceLabelUpdate();
    }
}
