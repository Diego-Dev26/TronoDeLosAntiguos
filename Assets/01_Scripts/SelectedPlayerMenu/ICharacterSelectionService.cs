using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterSelectionService
{
    string SelectedId { get; }
    bool HasSelection { get; }
    event Action<string> OnSelectedChanged; // id seleccionado o null
    void Select(string id);
    void Clear();
}
