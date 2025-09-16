using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOptionsService
{
    bool IsOpen { get; }
    event Action<bool> OnVisibilityChanged;
    void Show();
    void Hide();
    void ToggleOptions();
    void SetVisible(bool visible);
}