using UnityEngine;
using UnityEngine.Audio;

public class AudioProvider : AssetBaseProvider
{
    [Header("General")]
    [SerializeField] private AudioMixer masterMixer;
    [Header("Pool Setup")]
    [SerializeField] private GameObject audioSourcePrefab;
    [SerializeField] private int preloadCount = 20;

    [Header("Audio definition provider")]
    [SerializeField] private SoundDefinition defaultSoundDefinition;
    [SerializeField] private SoundCatalog soundCatalog;
    
    private static ISoundCatalogResolver resolver;
    private static AudioProvider instance;
    private DisposeBag disposeBag;

    public static AudioProvider Instance
    {
        get
        {
            if (instance == null)
            {
                Prewarm();
            }

            return instance;
        }
    }
    protected override AssetBaseProvider GetInstance()
    {
        return Instance;
    }
    
    public static void Prewarm()
    {
        if (instance != null)
        {
            return;
        }
        
        instance = Resources.Load<AudioProvider>(Strings.AssetProvidersPath + "Audio/AudioProvider");
        
        instance.poolObject = new GameObject {
            name = "AudioPool"
        };
        resolver = new SoundCatalogResolver(instance.soundCatalog);

        Instance.InstatiatePool(instance.audioSourcePrefab, instance.preloadCount);
        instance.disposeBag = new DisposeBag();
        DontDestroyOnLoad(instance.poolObject);
    }

    public AudioMixer GetMasterMixer()
    {
        return masterMixer;
    }
    public PooledAudioSource GetPooledSource()
    {
        var go = GetObjectFromPool(audioSourcePrefab);
        return go.GetComponent<PooledAudioSource>();
    }
    
    public SoundDefinition GetSoundDefinition(SoundId soundId)
    {
        if (resolver != null && resolver.TryGet(soundId.Value, out var def))
            return def;
        Debug.LogWarning($"[AudioProvider] Missing sound id '{soundId}'. Using default.", this);
        return defaultSoundDefinition;
    }
    public override void ReturnAllToPool()
    {
        disposeBag.Dispose();
        ReturnAllItemsToPool();
    }
}