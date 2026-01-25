using System.Collections.Generic;

public interface ISoundCatalogResolver
{
    bool TryGet(string id, out SoundDefinition def);
}

public class SoundCatalogResolver : ISoundCatalogResolver
{
    private readonly Dictionary<string, SoundDefinition> map;

    public SoundCatalogResolver(SoundCatalog catalog)
    {
        map = new Dictionary<string, SoundDefinition>();
        if (catalog == null) return;

        foreach (var e in catalog.entries)
        {
            if (e?.definition == null || string.IsNullOrEmpty(e.id)) continue;
            map[e.id] = e.definition;
        }
    }

    public bool TryGet(string id, out SoundDefinition def)
    {
        id = SoundCatalog.Normalize(id);
        return map.TryGetValue(id, out def);
    }
}