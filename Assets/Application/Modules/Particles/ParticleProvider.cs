using System;
using UnityEngine;

public class ParticleProvider : AssetBaseProvider
{
    public GameObject testParticle;

    private static ParticleProvider _instance;
    public DisposeBag disposeBag;

    public static ParticleProvider Instance
    {
        get
        {
            if (_instance == null)
            {
                Prewarm();
            }

            return _instance;
        }
    }
    protected override AssetBaseProvider GetInstance()
    {
        return Instance;
    }
    
    public static void Prewarm()
    {
        if (_instance != null)
        {
            return;
        }
        
        _instance = Resources.Load<ParticleProvider>(Strings.AssetProvidersPath + "ParticleProvider");
        
        _instance.poolObject = new GameObject {
            name = "ParticlePool"
        };
        
        foreach (ParticleType asset in Enum.GetValues(typeof(ParticleType))) {
            Instance.InstatiatePool(GameObjectForType(asset), PoolSizeForType(asset));
        }
        _instance.disposeBag = new DisposeBag();
        DontDestroyOnLoad(_instance.poolObject);
    }

    public static GameObject GetParticle(ParticleType asset)
    {
        return Instance.GetObjectFromPool(GameObjectForType(asset));
    }

    private static GameObject GameObjectForType(ParticleType type)
    {
        return type switch
        {
            ParticleType.Test => Instance.testParticle,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    private static int PoolSizeForType(ParticleType type)
    {
        return type switch
        {
            ParticleType.Test => 3,
            _ => 0
        };
    }
    public override void ReturnAllToPool()
    {
        disposeBag.Dispose();
        ReturnAllItemsToPool();
    }
}


public enum ParticleType
{
    Test, 
}