using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioService
{
    private AudioProvider provider;
    private AudioMixer masterMixer;

    private const string MasterParameter = "MasterVol";
    private const string MusicParameter = "MusicVol";
    private const string SFXParameter = "SFXVol";
    private const string UIParameter = "UIVol";
    private const string AmbienceParameter = "AmbienceVol";

    // defaults
    private float defaultSfxVolume = 1f;
    private float defaultMusicVolume = 1f;
    private float musicCrossfade = 1.25f; // in seconds

    private readonly List<PooledAudioSource> activeOneShots = new();
    private PooledAudioSource musicCurrent;
    private PooledAudioSource musicNext;
    private readonly List<PooledAudioSource> ambienceLoops = new();

    public AudioService()
    {
        this.provider = AudioProvider.Instance;
        this.masterMixer = this.provider.GetMasterMixer();
        ApplySavedMixerSettings();
    }

    #region Public API

    public void SetMixerValue(SoundMixerType mixerType, float value)
    {
        ServiceProvider.storage.SaveMixerValue(mixerType, value);
        ApplySavedMixerSettings();
    }

    public float GetMixerValue(SoundMixerType mixerType)
    {
        return ServiceProvider.storage.LoadMixerValue(mixerType);
    }

    public void PlayOneShot(SoundId soundType, Vector3? worldPos = null)
    {
        var def = provider.GetSoundDefinition(soundType);
        PlayOneShot(def, worldPos);
    }

    public PooledAudioSource PlayOneShot(SoundId soundType, Transform follow)
    {
        var def = provider.GetSoundDefinition(soundType);
        return PlayOneShot(def, follow);
    }

    public PooledAudioSource PlayAmbience(SoundId soundType, float fadeIn = 0.75f)
    {
        var def = provider.GetSoundDefinition(soundType);
        return PlayAmbience(def, fadeIn);
    }

    public void PlayMusic(SoundId soundType, float? crossfade = null)
    {
        var def = provider.GetSoundDefinition(soundType);
        PlayMusic(def, crossfade);
    }

    public void StopAmbience(PooledAudioSource src, float fadeOut = 0.75f)
    {
        if (src == null) return;
        if (ambienceLoops.Remove(src)) src.FadeOut(fadeOut);
    }

    public void FadeOutAll(float duration)
    {
        if (musicCurrent != null) musicCurrent.FadeOut(duration);
        foreach (var a in ambienceLoops) a.FadeOut(duration);
        foreach (var s in activeOneShots) s.FadeOut(duration * 0.5f);
    }

    public void FadeInMusicAndAmbience(float duration, float musicTarget = 1f, float ambTarget = 1f)
    {
        if (musicCurrent != null) musicCurrent.FadeToVolume(defaultMusicVolume * musicTarget, duration);
        foreach (var a in ambienceLoops)
            a.FadeToVolume(defaultMusicVolume * ambTarget, duration);
    }

    #endregion

    #region Internal API

    private void PlayOneShot(SoundDefinition def, Vector3? worldPos = null)
    {
        if (def == null || !def.HasAnyClip) return;
        var src = provider.GetPooledSource();
        SetupAndPlay(src, def, follow: null, atWorldPos: worldPos,
            startVolume: ResolveStartVolume(def), list: activeOneShots);
    }

    private PooledAudioSource PlayOneShot(SoundDefinition def, Transform follow)
    {
        if (def == null || !def.HasAnyClip) return null;
        var src = provider.GetPooledSource();
        SetupAndPlay(src, def, follow, null, ResolveStartVolume(def), activeOneShots);
        return src;
    }

    private void PlayMusic(SoundDefinition def, float? crossfade = null)
    {
        if (def == null || !def.HasAnyClip) return;

        float cf = crossfade ?? musicCrossfade;
        var next = provider.GetPooledSource();

        float target = ResolveStartVolume(def);
        SetupAndPlay(next, def, null, null, 0f, null);
        next.FadeToVolume(target, cf);

        if (musicCurrent != null)
        {
            musicCurrent.FadeOut(cf);
        }

        musicCurrent = next;
    }

    private PooledAudioSource PlayAmbience(SoundDefinition def, float fadeIn = 0.75f)
    {
        if (def == null || !def.HasAnyClip) return null;
        var src = provider.GetPooledSource();
        float target = ResolveStartVolume(def);
        SetupAndPlay(src, def, null, null, 0f, ambienceLoops);
        src.FadeToVolume(target, fadeIn);
        return src;
    }

    private void SetupAndPlay(
        PooledAudioSource src,
        SoundDefinition def,
        Transform follow,
        Vector3? atWorldPos,
        float startVolume,
        List<PooledAudioSource> list)
    {
        src.Play(def, follow, atWorldPos, startVolume, OnFinished);

        if (list != null) list.Add(src);

        void OnFinished(PooledAudioSource finished)
        {
            if (list != null) list.Remove(finished);
            var po = finished.Poolable;
            po.ReturnToPool();
        }
    }

    #endregion

    #region Other

    private float ResolveStartVolume(SoundDefinition def)
    {
        return def.channel switch
        {
            AudioChannel.Music => defaultMusicVolume * def.GetJitteredVolume(),
            AudioChannel.Ambience => defaultMusicVolume * def.GetJitteredVolume(),
            AudioChannel.UI => defaultSfxVolume * def.GetJitteredVolume(),
            AudioChannel.SFX => defaultSfxVolume * def.GetJitteredVolume(),
            _ => defaultSfxVolume * def.GetJitteredVolume(),
        };
    }

    private void ApplySavedMixerSettings()
    {
        SetMasterVolume(GetMixerValue(SoundMixerType.Master));
        SetMusicVolume(GetMixerValue(SoundMixerType.Music));
        SetAmbienceVolume(GetMixerValue(SoundMixerType.Music));
        SetSFXVolume(GetMixerValue(SoundMixerType.SFX));
        SetUIVolume(GetMixerValue(SoundMixerType.SFX));
    }

    private void SetMasterVolume(float linear01) => SetMixerLinear(MasterParameter, linear01);
    private void SetMusicVolume(float linear01) => SetMixerLinear(MusicParameter, linear01);
    private void SetSFXVolume(float linear01) => SetMixerLinear(SFXParameter, linear01);
    private void SetUIVolume(float linear01) => SetMixerLinear(UIParameter, linear01);
    private void SetAmbienceVolume(float linear01) => SetMixerLinear(AmbienceParameter, linear01);

    private void SetMixerLinear(string param, float linear01)
    {
        float dB = MixerVolumeUtility.LinearToDecibel(linear01);
        masterMixer.SetFloat(param, dB);
    }

    #endregion
}