using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionServiceMono : MonoBehaviour, ICharacterSelectionService
{
    private string _selectedId;

    public string SelectedId { get { return _selectedId; } }
    public bool HasSelection { get { return !string.IsNullOrEmpty(_selectedId); } }

    public event Action<string> OnSelectedChanged;

    public void Select(string id)
    {
        if (string.IsNullOrEmpty(id) || id == _selectedId) return;
        _selectedId = id;
        var h = OnSelectedChanged; if (h != null) h(_selectedId);
    }

    public void Clear()
    {
        if (_selectedId == null) return;
        _selectedId = null;
        var h = OnSelectedChanged; if (h != null) h(null);
    }
}
