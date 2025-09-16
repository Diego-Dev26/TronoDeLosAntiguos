using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsPanelMinimal : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider sldMaster;
    [SerializeField] private Slider sldMusic;
    [SerializeField] private Slider sldSfx;
    [SerializeField] private Toggle tglFullscreen;
    [SerializeField] private TMP_Dropdown ddQuality;
    [SerializeField] private TMP_Dropdown ddResolution;

    [Header("Audio Sources")]
    [Tooltip("Arrastra aquí el/los AudioSource que reproducen la MÚSICA del juego/menú")]
    [SerializeField] private AudioSource[] musicSources;
    [Tooltip("Arrastra aquí las fuentes de efectos (si ya tienes). Si no, déjalo vacío.")]
    [SerializeField] private AudioSource[] sfxSources;

    // Resoluciones (PC)
    private Resolution[] _resolutions;
    private int _resIndex;

    // Keys PlayerPrefs
    const string KMaster = "opt_master01";
    const string KMusic = "opt_music01";
    const string KSfx = "opt_sfx01";
    const string KFull = "opt_full";
    const string KQual = "opt_quality";
    const string KResW = "opt_res_w";
    const string KResH = "opt_res_h";

    private void Awake()
    {
        // ---- AUDIO ----
        float master = PlayerPrefs.GetFloat(KMaster, 0.8f);
        float music = PlayerPrefs.GetFloat(KMusic, 0.8f);
        float sfx = PlayerPrefs.GetFloat(KSfx, 0.8f);

        ApplyMaster(master, save: false);
        ApplyMusic(music, save: false);
        ApplySfx(sfx, save: false);

        if (sldMaster) { sldMaster.value = master; sldMaster.onValueChanged.AddListener(v => ApplyMaster(v)); }
        if (sldMusic) { sldMusic.value = music; sldMusic.onValueChanged.AddListener(v => ApplyMusic(v)); }
        if (sldSfx) { sldSfx.value = sfx; sldSfx.onValueChanged.AddListener(v => ApplySfx(v)); }

        // ---- VIDEO ----
        if (tglFullscreen)
        {
            bool full = PlayerPrefs.GetInt(KFull, Screen.fullScreen ? 1 : 0) == 1;
            Screen.fullScreen = full;
            tglFullscreen.isOn = full;
            tglFullscreen.onValueChanged.AddListener(OnFullscreen);
        }

        if (ddQuality)
        {
            ddQuality.ClearOptions();
            ddQuality.AddOptions(new List<string>(QualitySettings.names));
            int q = Mathf.Clamp(PlayerPrefs.GetInt(KQual, QualitySettings.GetQualityLevel()), 0, QualitySettings.names.Length - 1);
            ddQuality.value = q;
            ddQuality.RefreshShownValue();
            QualitySettings.SetQualityLevel(q);
            ddQuality.onValueChanged.AddListener(OnQuality);
        }

#if UNITY_ANDROID || UNITY_IOS
        if (ddResolution) ddResolution.gameObject.SetActive(false);
#else
        if (ddResolution)
        {
            // Lista única de resoluciones WxH
            List<Resolution> list = new List<Resolution>();
            foreach (var r in Screen.resolutions)
            {
                bool exists = false;
                for (int i = 0; i < list.Count; i++)
                    if (list[i].width == r.width && list[i].height == r.height) { exists = true; break; }
                if (!exists) list.Add(r);
            }
            _resolutions = list.ToArray();

            var opts = new List<string>();
            foreach (var r in _resolutions) opts.Add($"{r.width} x {r.height}");
            ddResolution.ClearOptions();
            ddResolution.AddOptions(opts);

            int sw = PlayerPrefs.GetInt(KResW, Screen.currentResolution.width);
            int sh = PlayerPrefs.GetInt(KResH, Screen.currentResolution.height);
            _resIndex = 0;
            for (int i = 0; i < _resolutions.Length; i++)
                if (_resolutions[i].width == sw && _resolutions[i].height == sh) { _resIndex = i; break; }

            ddResolution.value = _resIndex;
            ddResolution.RefreshShownValue();
            Screen.SetResolution(_resolutions[_resIndex].width, _resolutions[_resIndex].height, Screen.fullScreen);

            ddResolution.onValueChanged.AddListener(OnResolution);
        }
#endif
    }

    // ---------------- AUDIO ----------------
    private void ApplyMaster(float v, bool save = true)
    {
        v = Mathf.Clamp01(v);
        AudioListener.volume = v; // master global
        if (save) { PlayerPrefs.SetFloat(KMaster, v); PlayerPrefs.Save(); }
    }

    private void ApplyMusic(float v, bool save = true)
    {
        v = Mathf.Clamp01(v);
        if (musicSources != null)
            foreach (var s in musicSources) if (s) s.volume = v; // el Master lo aplica AudioListener
        if (save) { PlayerPrefs.SetFloat(KMusic, v); PlayerPrefs.Save(); }
    }

    private void ApplySfx(float v, bool save = true)
    {
        v = Mathf.Clamp01(v);
        if (sfxSources != null)
            foreach (var s in sfxSources) if (s) s.volume = v;
        if (save) { PlayerPrefs.SetFloat(KSfx, v); PlayerPrefs.Save(); }
    }

    // ---------------- VIDEO ----------------
    private void OnFullscreen(bool v)
    {
        Screen.fullScreen = v;
        PlayerPrefs.SetInt(KFull, v ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnQuality(int index)
    {
        index = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt(KQual, index);
        PlayerPrefs.Save();
    }

    private void OnResolution(int index)
    {
#if !UNITY_ANDROID && !UNITY_IOS
        if (_resolutions == null || _resolutions.Length == 0) return;
        _resIndex = Mathf.Clamp(index, 0, _resolutions.Length - 1);
        var r = _resolutions[_resIndex];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt(KResW, r.width);
        PlayerPrefs.SetInt(KResH, r.height);
        PlayerPrefs.Save();
#endif
    }
}
