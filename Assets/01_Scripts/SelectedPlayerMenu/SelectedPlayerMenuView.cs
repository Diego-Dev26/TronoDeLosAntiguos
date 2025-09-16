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
    private System.Action<string> _selHandler;

    private void Awake()
    {
        if (selectionService == null) selectionService = FindObjectOfType<CharacterSelectionServiceMono>();

        var sceneLoader = new SceneLoader();
        var persistence = new PlayerPrefsSelectionPersistence();
        ICharacterSelectionService sel = (ICharacterSelectionService)selectionService;

        _presenter = new SelectedPlayerMenuPresenter(sel, persistence, sceneLoader, gameSceneName);

        // Bot�n seleccionar deshabilitado al inicio
        SetConfirmInteractable(_presenter.CanConfirm());

        // Eventos UI (o con�ctalos v�a Inspector)
        if (btnSelected) btnSelected.onClick.AddListener(OnClickConfirm);

        // Escucha cambios de selecci�n para habilitar el bot�n
        selectionService.OnSelectedChanged += _ => SetConfirmInteractable(_presenter.CanConfirm());

        _selHandler = _ => SetConfirmInteractable(_presenter.CanConfirm());
        selectionService.OnSelectedChanged += _selHandler;

        // estado inicial
        SetConfirmInteractable(_presenter.CanConfirm());
    }

    private void OnDestroy()
    {
        if (selectionService != null && _selHandler != null)
            selectionService.OnSelectedChanged -= _selHandler;
    }

    private void SetConfirmInteractable(bool value)
    {
        if (btnSelected != null) btnSelected.interactable = value;

        // Tono menos claro cuando est� apagado
        if (btnSelectedCanvasGroup != null)
            btnSelectedCanvasGroup.alpha = value ? 1f : 0.5f;
    }

    public void OnClickConfirm()
    {
        _presenter.Confirm();
    }

    // Si necesitas exponer el "Regresar", con�ctalo a tu MainMenu:
    public void OnClickReturn()
    {
        // Aqu� solo emites un Debug. En tu men� principal ya tienes el m�todo para volver.
        Debug.Log("[SelectedPlayerMenu] Return pressed");
    }
}