using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PooledAudioSource : MonoBehaviour
{
    public AudioSource Source { get; private set; }
    public PoolableObject Poolable { get; private set; }
    private Action<PooledAudioSource> onFinished;
    private Transform followTarget;
    private bool isLooping;
    private bool isFading;

    void Awake()
    {
        Source = GetComponent<AudioSource>();
        Poolable = GetComponent<PoolableObject>();
    }

    public void Play(
        SoundDefinition def,
        Transform follow,
        Vector3? atWorldPos,
        float startVolume,
        Action<PooledAudioSource> finishedCb)
    {
        onFinished = finishedCb;
        followTarget = follow;

        var chosenClip = def?.GetRandomClip();
        if (def == null || chosenClip == null)
        {
            finishedCb?.Invoke(this);
            return;
        }

        Source.outputAudioMixerGroup = def.mixerGroup;
        Source.clip = chosenClip;
        Source.loop = def.loop;
        Source.playOnAwake = false;
        Source.pitch = def.GetJitteredPitch();
        Source.volume = startVolume;
        Source.spatialBlend = def.spatial3D ? def.spatialBlend : 0f;
        Source.minDistance = def.minDistance;
        Source.maxDistance = def.maxDistance;
        isLooping = def.loop;

        if (followTarget != null) {
            transform.SetParent(followTarget, false);
            transform.localPosition = Vector3.zero;
        } else if (atWorldPos.HasValue) {
            transform.position = atWorldPos.Value;
        }

        Source.Play();
        if (!isLooping && Source.clip != null)
            Invoke(nameof(CheckFinished), Source.clip.length + 0.05f);
    }

    void CheckFinished()
    {
        if (!isLooping && !isFading)
            onFinished?.Invoke(this);
    }

    public void StopImmediate()
    {
        CancelInvoke();
        Source.Stop();
        onFinished?.Invoke(this);
    }

    public void FadeOut(float duration)
    {
        if (isFading) return;
        isFading = true;
        StartCoroutine(FadeTo(0f, duration, StopImmediate));
    }

    public void FadeToVolume(float targetVolume, float duration, Action onDone = null)
    {
        StartCoroutine(FadeTo(targetVolume, duration, onDone));
    }

    private IEnumerator FadeTo(float target, float duration, Action onDone)
    {
        isFading = true;
        float start = Source.volume;
        float t = 0f;
        while (t < duration)
        {
            t += GameTicker.DeltaTime;
            Source.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        Source.volume = target;
        isFading = false;
        onDone?.Invoke();
    }
}
