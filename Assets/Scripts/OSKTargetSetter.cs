using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OSKTargetSetter : MonoBehaviour, IPointerClickHandler
{
    public OnScreenKeyboard keyboard;

    public void OnPointerClick(PointerEventData eventData)
    {
        TMP_InputField input = GetComponent<TMP_InputField>();
        if (keyboard != null && input != null)
        {
            keyboard.currentTarget = input;
            Debug.Log($"[OSK] Target ustawiony: {input.name}");
        }
    }
}
