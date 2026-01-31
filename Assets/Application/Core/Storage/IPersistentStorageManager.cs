using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPersistentStorageManager
{
    int LoadInt(string key);
    void SaveInt(string key, int value);
    
    float LoadFloat(string key);
    void SaveFloat(string key, float value);
    
    bool LoadBool(string key);
    void SaveBool(string key, bool value);
    
    // tutorial
    void SaveTutorial(string id, bool b);
    bool LoadTutorial(string id);
    
    // currency
    int LoadCurrency(CurrencyType type, int @default = 0);
    void SaveCurrency(CurrencyType type, int value);
    void SaveWallet(WalletAccount walletAccount, Dictionary<CurrencyType, int> balance);
    Dictionary<CurrencyType, int> LoadWallet(WalletAccount walletAccount, Dictionary<CurrencyType, int> defaultBalance);

    // audio
    float LoadMixerValue(SoundMixerType mixerType);
    void SaveMixerValue(SoundMixerType mixerType, float value);
    
    List<MaskType> LoadUnlockedMasks();
    void SaveUnlockedMasks(List<MaskType> masks);
}