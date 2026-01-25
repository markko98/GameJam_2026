#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundCatalog))]
public class SoundCatalogEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Regenerate SoundIds"))
        {
            SoundIdsCodeGenerator.Generate();
        }
    }
}
#endif