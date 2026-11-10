using System;
using UnityEngine;

/// <summary>
/// 音效管理。四个槽都可以留空——留空只是没声音，不会报错。
/// 音量存在 PlayerPrefs 里，重启游戏还记得。
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const string KeyVolume = "settings.volume";

    [Header("音效（可以留空）")]
    public AudioClip clickClip;
    public AudioClip upgradeClip;
    public AudioClip breakthroughClip;
    public AudioClip errorClip;

    [Range(0f, 1f)]
    public float volume = 0.8f;

    public event Action<float> OnVolumeChanged;

    AudioSource _source;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
        if (_source == null) _source = gameObject.AddComponent<AudioSource>();

        _source.playOnAwake = false;
        _source.spatialBlend = 0f;

        // 读上次设置的音量
        volume = PlayerPrefs.GetFloat(KeyVolume, volume);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------------- 音量 ----------------

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp(v, 0f, 1f);
        PlayerPrefs.SetFloat(KeyVolume, volume);
        PlayerPrefs.Save();

        OnVolumeChanged?.Invoke(volume);
    }

    /// <summary>循环切换：100 → 80 → 60 → 40 → 20 → 0(静音) → 100</summary>
    public void CycleVolume()
    {
        const float step = 0.2f;
        float next = Mathf.Round(volume / step) * step - step;

        if (next < -0.001f) next = 1f;
        if (next > 1f) next = 1f;

        SetVolume(next);
        PlayClick();
    }

    public string GetVolumeLabel()
    {
        if (volume <= 0.001f) return "音量：静音";
        return $"音量：{Mathf.RoundToInt(volume * 100f)}%";
    }

    // ---------------- 播放 ----------------

    public void PlayClick()        => Play(clickClip);
    public void PlayUpgrade()      => Play(upgradeClip);
    public void PlayBreakthrough() => Play(breakthroughClip);
    public void PlayError()        => Play(errorClip);

    void Play(AudioClip clip)
    {
        if (clip == null || _source == null) return;
        if (volume <= 0.001f) return;

        _source.PlayOneShot(clip, volume);
    }

    public static void Click()        { if (Instance != null) Instance.PlayClick(); }
    public static void Upgrade()      { if (Instance != null) Instance.PlayUpgrade(); }
    public static void Breakthrough() { if (Instance != null) Instance.PlayBreakthrough(); }
    public static void Error()        { if (Instance != null) Instance.PlayError(); }
}
