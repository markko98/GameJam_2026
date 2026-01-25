using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound Catalog", fileName = "SoundCatalog")]
public class SoundCatalog : ScriptableObject
{
    public List<Entry> entries = new();

    [System.Serializable]
    public class Entry
    {
        public string id;
        public SoundDefinition definition;
    }

    private void OnValidate()
    {
        foreach (var e in entries)
        {
            if (e == null) continue;
            if (!string.IsNullOrEmpty(e.id)) e.id = Normalize(e.id);
        }

        var duplicates = entries
            .Where(e => e != null && !string.IsNullOrEmpty(e.id))
            .GroupBy(e => e.id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            Debug.LogError($"[SoundCatalog] Duplicate IDs: {string.Join(", ", duplicates)}", this);
        }
    }

    public static string Normalize(string s) => s?.Trim().ToLowerInvariant();
}