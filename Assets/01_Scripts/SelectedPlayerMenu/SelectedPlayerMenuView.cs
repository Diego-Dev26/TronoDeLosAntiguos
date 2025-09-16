using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedPlayerMenuView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button btnSelected;   // "SELECCIONAR"
    [SerializeField] private Button btnReturn;     // "REGRESAR"
    [SerializeField] private CanvasGroup btnSelectedCanvasGroup; // opcional para tono apagado

    [Header("Config")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Servicios")]
    [SerializeField] private CharacterSelectionServiceMono selectionService;
    private SelectedPlayerMenuPresenter _presenter;

    private void Awake()
    {
        if (selectionService == null) selectionService = FindObjectOfType<CharacterSelectionServiceMono>();

        var sceneLoader = new SceneLoader();
        var persistence = new PlayerPrefsSelectionPersistence();
        ICharacterSelectionService sel = (ICharacterSelectionService)selectionService;

        _presenter = new SelectedPlayerMenuPresenter(sel, persistence, sceneLoader, gameSceneName);

        // Botón seleccionar deshabilitado al inicio
        SetConfirmInteractable(_presenter.CanConfirm());

        // Eventos UI (o conéctalos vía Inspector)
        if (btnSelected) btnSelected.onClick.AddListener(OnClickConfirm);

        // Escucha cambios de selección para habilitar el botón
        selectionService.OnSelectedChanged += _ => SetConfirmInteractable(_presenter.CanConfirm());
    }

    private void OnDestroy()
    {
        if (selectionService != null)
            selectionService.OnSelectedChanged -= _ => SetConfirmInteractable(_presenter.CanConfirm());
    }

    private void SetConfirmInteractable(bool value)
    {
        if (btnSelected != null) btnSelected.interactable = value;

        // Tono menos claro cuando está apagado
        if (btnSelectedCanvasGroup != null)
            btnSelectedCanvasGroup.alpha = value ? 1f : 0.5f;
    }

    public void OnClickConfirm()
    {
        _presenter.Confirm();
    }

    // Si necesitas exponer el "Regresar", conéctalo a tu MainMenu:
    public void OnClickReturn()
    {
        // Aquí solo emites un Debug. En tu menú principal ya tienes el método para volver.
        Debug.Log("[SelectedPlayerMenu] Return pressed");
    }
}