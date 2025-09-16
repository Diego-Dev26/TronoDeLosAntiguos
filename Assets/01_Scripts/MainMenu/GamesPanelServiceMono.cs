using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamesPanelServiceMono : MonoBehaviour, IGamesPanelService
{
    [SerializeField] private GameObject gamesPanel;

    public bool IsOpen { get { return gamesPanel != null && gamesPanel.activeSelf; } }

    public event Action<bool> OnVisibilityChanged;

    public void Show() { SetVisible(true); }
    public void Hide() { SetVisible(false); }
    public void Toggle() { SetVisible(!IsOpen); }

    public void SetVisible(bool visible)
    {
        if (gamesPanel == null) return;
        if (gamesPanel.activeSelf == visible) return;

        gamesPanel.SetActive(visible);

        var h = OnVisibilityChanged;
        if (h != null) h(visible);
    }
}