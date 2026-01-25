#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatisticsCatalogue))]
public sealed class StatisticsCatalogueEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var catalog = (StatisticsCatalogue)target;

        EditorGUILayout.Space(8);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button($"Generate {catalog.generatedFileName}", GUILayout.Height(28)))
            {
                catalog.GenerateStatIds();
            }
        }

        EditorGUILayout.HelpBox(
            $"Click 'Generate {catalog.generatedFileName}' to update the constants file from this catalog.",
            MessageType.Info);
    }
}
#endif