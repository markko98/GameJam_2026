using System.Collections.Generic;
using UnityEngine;

public static class TutorialAnchorRegistry
{
    private static readonly Dictionary<string, TutorialAnchor> anchorRegistry = new();

    public static void Register(TutorialAnchor anchor)
    {
        if (anchor == null || string.IsNullOrEmpty(anchor.AnchorId)) return;
        anchorRegistry[anchor.AnchorId] = anchor;
    }

    public static void Unregister(TutorialAnchor anchor)
    {
        if (anchor == null || string.IsNullOrEmpty(anchor.AnchorId)) return;
        if (anchorRegistry.TryGetValue(anchor.AnchorId, out var current) && current == anchor)
            anchorRegistry.Remove(anchor.AnchorId);
    }

    public static TutorialAnchor Get(string anchorId)
        => string.IsNullOrEmpty(anchorId) ? null : (anchorRegistry.GetValueOrDefault(anchorId));
}