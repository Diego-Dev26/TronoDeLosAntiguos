using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPresenter : MonoBehaviour
{
    private readonly ISceneLoader _sceneLoader;
    private readonly IApplicationController _app;
    private readonly IOptionsService _options;
    private readonly IGamesPanelService _games;
    private readonly string _nombreEscenaJuego;

    public MainMenuPresenter(ISceneLoader sceneLoader,
                             IApplicationController app,
                             IOptionsService options,
                             IGamesPanelService games,
                             string nombreEscenaJuego)
    {
        _sceneLoader = sceneLoader;
        _app = app;
        _options = options;
        _games = games;
        _nombreEscenaJuego = nombreEscenaJuego;
    }

    public void Jugar() => _games.Show();

    public void Opciones() => _options.ToggleOptions();

    public void Salir() => _app.Quit();

    public void IniciarJuegoDesdeGames() => _sceneLoader.Load(_nombreEscenaJuego);

    public void CerrarGames() => _games.Hide();
    public void CerrarOptions() => _options.Hide();
}

