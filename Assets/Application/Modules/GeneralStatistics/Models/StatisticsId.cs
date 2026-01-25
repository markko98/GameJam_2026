using System;
using UnityEngine;

[Serializable]
public struct StatisticsId
{
    [SerializeField] private string value;
    public string Value => value;

    public StatisticsId(string id) => value = id;

    public static implicit operator string(StatisticsId id) => id.value;
    public static implicit operator StatisticsId(string id) => new(id);

    public override string ToString() => value;
}