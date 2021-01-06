using System;
using UnityEngine;

public class Buttons : MonoBehaviour
{
    public enum ButtonAction { RESET, PAUSE, SWITCH };
    public static event Action<ButtonAction> OnPress;

    public void ResetButton()
    {
        OnPress?.Invoke(ButtonAction.RESET);
    }

    public void PauseButton()
    {
        OnPress?.Invoke(ButtonAction.PAUSE);
    }

    public void SwitchButton()
    {
        OnPress?.Invoke(ButtonAction.SWITCH);
    }
}
