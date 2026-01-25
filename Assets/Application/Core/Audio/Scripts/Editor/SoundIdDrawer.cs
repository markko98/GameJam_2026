#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SoundId))]
public class SoundIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valueProp = property.FindPropertyRelative("value");
        string current = valueProp.stringValue;

        var catalog = AssetDatabase.FindAssets("t:SoundCatalog")
            .Select(g => AssetDatabase.LoadAssetAtPath<SoundCatalog>(AssetDatabase.GUIDToAssetPath(g)))
            .FirstOrDefault();

        if (catalog == null)
        {
            EditorGUI.PropertyField(position, valueProp, label, true);
            return;
        }

        var ids = catalog.entries
            .Where(e => e != null && !string.IsNullOrEmpty(e.id))
            .Select(e => e.id)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        int currentIndex = Mathf.Max(0, ids.IndexOf(SoundCatalog.Normalize(current)));
        int newIndex = EditorGUI.Popup(position, label, currentIndex, ids.Select(i => new GUIContent(i)).ToArray());

        if (newIndex >= 0 && newIndex < ids.Count)
            valueProp.stringValue = ids[newIndex];
    }
}
#endif