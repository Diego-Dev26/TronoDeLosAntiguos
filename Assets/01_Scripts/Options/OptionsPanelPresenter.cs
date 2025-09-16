using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsPanelPresenter
{
    private readonly IAudioSettings _audio;
    private readonly IVideoSettings _video;

    public OptionsPanelPresenter(IAudioSettings audio, IVideoSettings video)
    {
        _audio = audio;
        _video = video;
    }

    // AUDIO
    public float GetMaster01() { return _audio.Master01; }
    public float GetMusic01() { return _audio.Music01; }
    public float GetSfx01() { return _audio.Sfx01; }
    public void SetMaster01(float v) { _audio.SetMaster01(v); }
    public void SetMusic01(float v) { _audio.SetMusic01(v); }
    public void SetSfx01(float v) { _audio.SetSfx01(v); }

    // VIDEO
    public bool GetFullscreen() { return _video.Fullscreen; }
    public void SetFullscreen(bool v) { _video.SetFullscreen(v); }
    public int GetQualityIndex() { return _video.QualityIndex; }
    public void SetQuality(int index) { _video.SetQuality(index); }
    public int GetResolutionIndex() { return _video.ResolutionIndex; }
    public void SetResolution(int index) { _video.SetResolution(index); }
    public UnityEngine.Resolution[] GetResolutions() { return _video.AvailableResolutions; }
}
