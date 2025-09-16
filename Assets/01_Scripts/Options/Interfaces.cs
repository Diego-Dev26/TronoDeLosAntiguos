using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISettingsPersistence
{
    void Save(string key, float value);
    void Save(string key, int value);
    void Save(string key, bool value);

    float LoadFloat(string key, float def);
    int LoadInt(string key, int def);
    bool LoadBool(string key, bool def);
}

public interface IAudioSettings
{
    float Master01 { get; }
    float Music01 { get; }
    float Sfx01 { get; }

    event Action OnChanged;

    void SetMaster01(float v01);
    void SetMusic01(float v01);
    void SetSfx01(float v01);
}

public interface IVideoSettings
{
    bool Fullscreen { get; }
    int QualityIndex { get; }       // QualitySettings.currentLevel
    int ResolutionIndex { get; }       // índice en AvailableResolutions
    Resolution[] AvailableResolutions { get; }

    event Action OnChanged;

    void SetFullscreen(bool value);
    void SetQuality(int index);
    void SetResolution(int index); // usa AvailableResolutions[index]
}
