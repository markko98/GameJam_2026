using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalPersistentStorageManager : IPersistentStorageManager
{
    public int LoadInt(string key)
    {
        return Serializer.LoadFromPlayerPrefs<int>(key);
    }

    public void SaveInt(string key, int value)
    {
        Serializer.SaveToPlayerPrefs(key, value);
    }

    public float LoadFloat(string key)
    {
        return Serializer.LoadFromPlayerPrefs<float>(key);
    }

    public void SaveFloat(string key, float value)
    {
        Serializer.SaveToPlayerPrefs(key, value);
    }

    public bool LoadBool(string key)
    {
        return Serializer.LoadFromPlayerPrefs<bool>(key);
    }

    public void SaveBool(string key, bool value)
    {
        Serializer.SaveToPlayerPrefs(key, value);
    }
    

    public void SaveTutorial(string id, bool isCompleted)
    {
        PlayerPrefs.SetInt(StorageKeys.TutorialPrefixKey + id, Utils.BoolToInt(isCompleted));
    } 
    public bool LoadTutorial(string id)
    {
        return Utils.IntToBool(PlayerPrefs.GetInt(StorageKeys.TutorialPrefixKey + id));
    }

    // currency    
    private static string Key(CurrencyType t) => $"{StorageKeys.CurrencySufix}.{t.ToString().ToLower()}";
    private static string Key(WalletAccount t) => $"{StorageKeys.CurrencyWalletSufix}.{t.ToString().ToLower()}";
    
    public int LoadCurrency(CurrencyType type, int @default = 0)
    {
        return PlayerPrefs.GetInt(Key(type), @default);
    }

    public void SaveCurrency(CurrencyType type, int value)
    {
        PlayerPrefs.SetInt(Key(type), Mathf.Clamp(value, int.MinValue, int.MaxValue));
    }

    public void SaveWallet(WalletAccount walletAccount, Dictionary<CurrencyType, int> balance)
    {
        Serializer.SaveToPlayerPrefs(Key(walletAccount), balance);
    }

    public Dictionary<CurrencyType, int> LoadWallet(WalletAccount walletAccount, Dictionary<CurrencyType, int> defaultBalance)
    {
        return Serializer.LoadFromPlayerPrefs<Dictionary<CurrencyType, int>>(Key(walletAccount)) ?? defaultBalance;
    }

    // audio
    public float LoadMixerValue(SoundMixerType mixerType)
    {
        var value = PlayerPrefs.GetFloat($"{StorageKeys.MixerValue}_{mixerType.ToString()}", 1);
        return value;
    }

    public void SaveMixerValue(SoundMixerType mixerType, float value)
    {
        PlayerPrefs.SetFloat($"{StorageKeys.MixerValue}_{mixerType.ToString()}", value);
    }

    public List<MaskType> LoadUnlockedMasks()
    {
        return Serializer.LoadFromPlayerPrefs<List<MaskType>>(StorageKeys.UnlockedMasks) ?? new List<MaskType>();
    }

    public void SaveUnlockedMasks(List<MaskType> masks)
    {
        Serializer.SaveToPlayerPrefs(StorageKeys.UnlockedMasks, masks);
    }
}