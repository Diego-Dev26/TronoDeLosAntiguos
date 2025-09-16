using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsSettingsPersistence : ISettingsPersistence
{
    public void Save(string key, float value) { PlayerPrefs.SetFloat(key, value); PlayerPrefs.Save(); }
    public void Save(string key, int value) { PlayerPrefs.SetInt(key, value); PlayerPrefs.Save(); }
    public void Save(string key, bool value) { PlayerPrefs.SetInt(key, value ? 1 : 0); PlayerPrefs.Save(); }

    public float LoadFloat(string key, float def) { return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : def; }
    public int LoadInt(string key, int def) { return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : def; }
    public bool LoadBool(string key, bool def) { return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) == 1 : def; }
}
