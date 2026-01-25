using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class StatisticsData
{
    private int version = 1;
    private Dictionary<string, int> intValues = new();
    private Dictionary<string, float> floatValues = new();

    public float GetFloatValue(string key) => floatValues.GetValueOrDefault(key, 0f);
    public int GetIntValue(string key) => intValues.GetValueOrDefault(key, 0);

    public void AddFloatValue(string key, float addedValue) => floatValues[key] = GetFloatValue(key) + addedValue;
    public void SetFloatValue(string key, float value) => floatValues[key] = value;
    public void SetMaxFloatValue(string key, float maxValue) => floatValues[key] = Math.Max(GetFloatValue(key), maxValue);

    public void AddIntValue(string key, int addedValue) => intValues[key] = GetIntValue(key) + addedValue;
    public void SetIntValue(string key, int value) => intValues[key] = value;
    public void SetMaxIntValue(string key, int maxValue) => intValues[key] = Math.Max(GetIntValue(key), maxValue);

    public void MigrateIfNeeded()
    {
    }
}