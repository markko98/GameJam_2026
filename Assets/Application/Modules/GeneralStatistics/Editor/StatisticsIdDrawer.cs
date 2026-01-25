#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatisticsId))]
public sealed class StatisticsIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valueProp = property.FindPropertyRelative("value");
        string current = valueProp.stringValue;

        var catalog = AssetDatabase.FindAssets("t:StatisticsCatalog")
            .Select(g => AssetDatabase.LoadAssetAtPath<StatisticsCatalogue>(AssetDatabase.GUIDToAssetPath(g)))
            .FirstOrDefault();

        if (catalog == null)
        {
            EditorGUI.PropertyField(position, valueProp, label, true);
            return;
        }

        var ids = catalog.entries
            .Where(e => e != null && !string.IsNullOrEmpty(e.id))
            .Select(e => StatisticsCatalogue.Normalize(e.id))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        int currentIndex = Mathf.Max(0, ids.IndexOf(StatisticsCatalogue.Normalize(current)));
        var options = ids.Select(i => new GUIContent(i).text).ToArray();
        var newIndex = EditorGUI.Popup(position, label.text, currentIndex, options);
        if (newIndex >= 0 && newIndex < ids.Count)
            valueProp.stringValue = ids[newIndex];
    }
}
#endif