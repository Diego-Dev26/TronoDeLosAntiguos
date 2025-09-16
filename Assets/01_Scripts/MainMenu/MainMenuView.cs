using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button btnJugar;
    [SerializeField] private Button btnOpciones;
    [SerializeField] private Button btnSalir;

    [Header("Panels")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private OptionsServiceMono optionsService;
    [SerializeField] private GamesPanelServiceMono gamesService;

    [Header("Configuración")]
    [SerializeField] private string nombreEscenaJuego = "SelectedPlayerMenu";

    private MainMenuPresenter _presenter;
    private IOptionsService _opts;
    private IGamesPanelService _games;

    private void Awake()
    {
        var sceneLoader = new SceneLoader();
        var app = new UnityApplication();

        if (optionsService == null) optionsService = FindObjectOfType<OptionsServiceMono>();
        if (gamesService == null) gamesService = FindObjectOfType<GamesPanelServiceMono>();

        _opts = (optionsService as IOptionsService) ?? new NullOptionsService();
        _games = (gamesService as IGamesPanelService) ?? new NullGamesPanelService();

        _presenter = new MainMenuPresenter(sceneLoader, app, _opts, _games, nombreEscenaJuego);

        if (btnJugar) btnJugar.onClick.AddListener(OnClickJugar);
        if (btnOpciones) btnOpciones.onClick.AddListener(OnClickOpciones);
        if (btnSalir) btnSalir.onClick.AddListener(OnClickSalir);

        _opts.OnVisibilityChanged += OnOptionsVisibilityChanged;
        _games.OnVisibilityChanged += OnGamesVisibilityChanged;

        ActualizarPanelMain();
    }

    private void OnDestroy()
    {
        if (_opts != null) _opts.OnVisibilityChanged -= OnOptionsVisibilityChanged;
        if (_games != null) _games.OnVisibilityChanged -= OnGamesVisibilityChanged;
    }

    // --- Pasarelas públicas ---
    public void OnClickJugar() { _presenter.Jugar(); }
    public void OnClickOpciones() { _presenter.Opciones(); }
    public void OnClickSalir() { _presenter.Salir(); }

    public void OnClickIniciarJuego() { _presenter.IniciarJuegoDesdeGames(); }
    public void OnClickCerrarGames() { _presenter.CerrarGames(); }

    private void OnOptionsVisibilityChanged(bool visible)
    {
        if (visible) _games.SetVisible(false);
        ActualizarPanelMain();
    }

    private void OnGamesVisibilityChanged(bool visible)
    {
        if (visible) _opts.SetVisible(false); 
        ActualizarPanelMain();
    }

    private void ActualizarPanelMain()
    {
        if (panelMain != null)
        {
            bool mostrarMain = !_opts.IsOpen && !_games.IsOpen;
            panelMain.SetActive(mostrarMain);
        }
    }

    // Implementaciones nulas para respetar SOLID sin null checks
    private class NullOptionsService : IOptionsService
    {
        public bool IsOpen { get { return false; } }
        public event Action<bool> OnVisibilityChanged { add { } remove { } }
        public void Show() { }
        public void Hide() { }
        public void ToggleOptions() { }
        public void SetVisible(bool visible) { }

    }
    private class NullGamesPanelService : IGamesPanelService
    {
        public bool IsOpen { get { return false; } }
        public event Action<bool> OnVisibilityChanged { add { } remove { } }
        public void Show() { }
        public void Hide() { }
        public void Toggle() { }
        public void SetVisible(bool visible) { }
    }
}