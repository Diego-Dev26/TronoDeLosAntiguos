using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsSelectionPersistence : ISelectionPersistence
{
    private const string Key = "selected_character_id";

    public void Save(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        UnityEngine.PlayerPrefs.SetString(Key, id);
        UnityEngine.PlayerPrefs.Save();
    }

    public string Load()
    {
        return UnityEngine.PlayerPrefs.GetString(Key, null);
    }

    public void Clear()
    {
        UnityEngine.PlayerPrefs.DeleteKey(Key);
    }
}
