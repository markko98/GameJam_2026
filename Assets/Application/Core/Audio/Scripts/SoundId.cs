using System;
using UnityEngine;

[Serializable]
public struct SoundId : IEquatable<SoundId>
{
    [SerializeField] private string value;

    public SoundId(string id) { value = SoundCatalog.Normalize(id); }
    public string Value => value;

    public override string ToString() => value ?? string.Empty;
    public bool Equals(SoundId other) => string.Equals(value, other.value, StringComparison.Ordinal);
    public override bool Equals(object obj) => obj is SoundId other && Equals(other);
    public override int GetHashCode() => (value ?? string.Empty).GetHashCode();

    public static implicit operator string(SoundId id) => id.Value;
    public static implicit operator SoundId(string id) => new SoundId(id);
}