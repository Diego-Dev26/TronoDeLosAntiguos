using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedPlayerMenuPresenter
{
    private readonly ICharacterSelectionService _selection;
    private readonly ISelectionPersistence _persistence;
    private readonly ISceneLoader _sceneLoader;
    private readonly string _gameSceneName;

    public SelectedPlayerMenuPresenter(ICharacterSelectionService selection,
                                       ISelectionPersistence persistence,
                                       ISceneLoader sceneLoader,
                                       string gameSceneName)
    {
        _selection = selection;
        _persistence = persistence;
        _sceneLoader = sceneLoader;
        _gameSceneName = gameSceneName;
    }

    public bool CanConfirm() { return _selection.HasSelection; }

    public void Confirm()
    {
        if (!_selection.HasSelection) return;
        _persistence.Save(_selection.SelectedId);
        _sceneLoader.Load(_gameSceneName);
    }
}
