using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsServiceMono : MonoBehaviour, IOptionsService
{
    [SerializeField] private GameObject panelOpciones;

    public bool IsOpen { get { return panelOpciones != null && panelOpciones.activeSelf; } }

    public event Action<bool> OnVisibilityChanged;
    public void Show() { SetVisible(true); }
    public void Hide() { SetVisible(false); }
    public void ToggleOptions()
    {
        SetVisible(!IsOpen);
    }

    public void SetVisible(bool visible)
    {
        if (panelOpciones == null) return;
        if (panelOpciones.activeSelf == visible) return;

        panelOpciones.SetActive(visible);

        var handler = OnVisibilityChanged;
        if (handler != null) handler(visible);
    }
}