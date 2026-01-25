using UnityEngine;

public class AudioPlayerBridge : MonoBehaviour
{
    public SoundId soundId;
    public bool playAtAwake;
    public bool playAtEnable;

    [Header("Attach / Spatial")]
    public bool attachToThisTransform;
    public bool useWorldPosition;
    private PooledAudioSource pooledSound;

    void Awake()
    {
        if (playAtAwake) Play();
    }

    void OnEnable()
    {
        if (playAtEnable) Play();
    }

    public void Play()
    {
        if (attachToThisTransform)
        {
            pooledSound = ServiceProvider.audioService.PlayOneShot(soundId, transform);
        }
        else if (useWorldPosition)
        {
            ServiceProvider.audioService.PlayOneShot(soundId, transform.position);
        }
        else
        {
            ServiceProvider.audioService.PlayOneShot(soundId);
        }
    }

    public void PlayUI()
    {
        ServiceProvider.audioService.PlayOneShot(soundId);
    }

    private void OnDestroy()
    {
        pooledSound?.Poolable.ReturnToPool();
    }
}