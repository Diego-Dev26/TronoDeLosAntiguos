using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGamesPanelService
{
    bool IsOpen { get; }
    event Action<bool> OnVisibilityChanged;
    void Show();
    void Hide();
    void Toggle();
    void SetVisible(bool visible);
}
