using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/Sound Definition", fileName = "Sound_")]
public class SoundDefinition : ScriptableObject
{
    public List<AudioClip> clips = new();

    [Header("Routing")] public AudioMixerGroup mixerGroup;
    public AudioChannel channel = AudioChannel.SFX;

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitch = 1f;
    [Range(0f, 2f)] public float volumeGain = 0f;
    
    public bool loop;
    public bool playOnAwake;
    public bool spatial3D = false;
    [ShowIf("spatial3D")] [Range(0f, 1f)] public float spatialBlend = 1f;
    [ShowIf("spatial3D")] public float minDistance = 1f;
    [ShowIf("spatial3D")] public float maxDistance = 100f;

    [Header("Variations (optional)")] public Vector2 volumeJitter = Vector2.zero;
    public Vector2 pitchJitter = Vector2.zero;
    public bool HasAnyClip => clips != null && clips.Count > 0;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Count == 0) return null;
        if (clips.Count == 1) return clips[0];
        return clips.GetRandomElement();
    }

    public float GetJitteredVolume()
    {
        if (volumeJitter == Vector2.zero) return volume + volumeGain;
        return Mathf.Clamp01(volume + Random.Range(volumeJitter.x, volumeJitter.y)) + volumeGain;
    }

    public float GetJitteredPitch()
    {
        if (pitchJitter == Vector2.zero) return pitch;
        return Mathf.Clamp(pitch + Random.Range(pitchJitter.x, pitchJitter.y), -3f, 3f);
    }
}