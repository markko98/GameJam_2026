#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class LevelSODefinitionWindow : EditorWindow
{
    private LevelSO level;
    private SerializedObject so;

    private PlayerSide editingSide = PlayerSide.Left;

    private BlockType paintType = BlockType.Floor;
    private bool isPainting;

    private Vector2 scroll;

    // visuals
    private float cellPx = 34f;
    private float cellPad = 3f;

    [MenuItem("Tools/Levels/LevelSO Editor")]
    public static void Open()
    {
        var w = GetWindow<LevelSODefinitionWindow>("LevelSO Editor");
        w.minSize = new Vector2(520, 520);
        w.Show();
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is LevelSO selected)
        {
            level = selected;
            so = level ? new SerializedObject(level) : null;
            Repaint();
        }
    }

    private void OnGUI()
    {
        DrawHeader();

        if (!level)
        {
            EditorGUILayout.HelpBox("Assign a LevelSO to start editing.", MessageType.Info);
            return;
        }

        if (so == null) so = new SerializedObject(level);
        so.Update();

        EditorGUILayout.Space(8);
        DrawSideSelector();

        var playerProp = GetPlayerProp(editingSide);
        if (playerProp == null)
        {
            EditorGUILayout.HelpBox("Player definition is missing in LevelSO.", MessageType.Error);
            so.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space(6);
        DrawGridSettings(playerProp);

        EditorGUILayout.Space(8);
        DrawPalette();

        EditorGUILayout.Space(8);
        DrawGrid(playerProp);

        EditorGUILayout.Space(10);
        DrawBottomActions(playerProp);

        so.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(level);
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("LevelSO Visual Editor", EditorStyles.boldLabel);

        var newLevel = (LevelSO)EditorGUILayout.ObjectField("LevelSO", level, typeof(LevelSO), false);
        if (newLevel != level)
        {
            level = newLevel;
            so = level ? new SerializedObject(level) : null;
        }

        if (level)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset", AssetDatabase.GetAssetPath(level));
            if (GUILayout.Button("Ping", GUILayout.Width(60)))
                EditorGUIUtility.PingObject(level);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSideSelector()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Editing", GUILayout.Width(60));

        var newSide = (PlayerSide)GUILayout.Toolbar((int)editingSide, new[] { "Left", "Right" });
        if (newSide != editingSide)
        {
            editingSide = newSide;
            isPainting = false;
        }

        GUILayout.FlexibleSpace();
        cellPx = EditorGUILayout.Slider("Cell Size", cellPx, 18f, 60f);

        EditorGUILayout.EndHorizontal();
    }

    private SerializedProperty GetPlayerProp(PlayerSide side)
    {
        return side == PlayerSide.Left
            ? so.FindProperty("leftPlayer")
            : so.FindProperty("rightPlayer");
    }

    private void DrawGridSettings(SerializedProperty playerProp)
    {
        var sizeProp = playerProp.FindPropertyRelative("gridSize");
        var cellSizeProp = playerProp.FindPropertyRelative("cellSize");
        var startProp = playerProp.FindPropertyRelative("playerStartCell");
        var targetProp = playerProp.FindPropertyRelative("targetCell");
        var cellsProp = playerProp.FindPropertyRelative("cells");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(sizeProp, new GUIContent("Grid Size (W,H)"));
        EditorGUILayout.PropertyField(cellSizeProp, new GUIContent("Cell Size (World)"));
        EditorGUILayout.PropertyField(startProp, new GUIContent("Player Start Cell"));
        EditorGUILayout.PropertyField(targetProp, new GUIContent("Target Cell"));

        if (EditorGUI.EndChangeCheck())
        {
            EnsureCellsArraySize(playerProp, cellsProp, sizeProp.vector2IntValue);
        }

        var sz = sizeProp.vector2IntValue;
        int required = Mathf.Max(1, sz.x * sz.y);
        EditorGUILayout.LabelField($"Cells: {cellsProp.arraySize} / Required: {required}");

        EditorGUILayout.EndVertical();
    }

    private void EnsureCellsArraySize(SerializedProperty playerProp, SerializedProperty cellsProp, Vector2Int size)
    {
        int w = Mathf.Max(1, size.x);
        int h = Mathf.Max(1, size.y);
        int required = w * h;

        playerProp.FindPropertyRelative("gridSize").vector2IntValue = new Vector2Int(w, h);

        if (cellsProp.arraySize == required) return;

        Undo.RecordObject(level, "Resize Grid Cells");

        int old = cellsProp.arraySize;
        cellsProp.arraySize = required;

        // Init defaults
        for (int i = old; i < required; i++)
        {
            var el = cellsProp.GetArrayElementAtIndex(i);
            el.enumValueIndex = (int)BlockType.Floor;
        }

        so.ApplyModifiedProperties();
        so.Update();
        EditorUtility.SetDirty(level);
    }

    private void DrawPalette()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Paint Palette", EditorStyles.boldLabel);

        paintType = (BlockType)EditorGUILayout.EnumPopup("Block Type", paintType);

        EditorGUILayout.HelpBox(
            "Left Click = paint. Drag = paint multiple. Alt+Click = pick from cell.\n" +
            "Right Click cell = context menu for Start/Target.",
            MessageType.Info);

        EditorGUILayout.EndVertical();
    }

    private void DrawGrid(SerializedProperty playerProp)
    {
        var size = playerProp.FindPropertyRelative("gridSize").vector2IntValue;
        var cellsProp = playerProp.FindPropertyRelative("cells");

        int w = Mathf.Max(1, size.x);
        int h = Mathf.Max(1, size.y);
        int required = w * h;

        if (cellsProp.arraySize != required)
        {
            EnsureCellsArraySize(playerProp, cellsProp, size);
            so.ApplyModifiedProperties();
            so.Update();
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Visual Grid", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        var e = Event.current;

        for (int y = h - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(6);

            for (int x = 0; x < w; x++)
            {
                int index = y * w + x;
                var cellEnumProp = cellsProp.GetArrayElementAtIndex(index);

                BlockType bt = (BlockType)cellEnumProp.enumValueIndex;

                Rect r = GUILayoutUtility.GetRect(cellPx, cellPx, GUILayout.Width(cellPx), GUILayout.Height(cellPx));
                r = new Rect(r.x + cellPad * 0.5f, r.y + cellPad * 0.5f, r.width - cellPad, r.height - cellPad);

                Color prev = GUI.color;
                GUI.color = GetCellColor(bt);
                GUI.Box(r, GUIContent.none);
                GUI.color = prev;

                var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.black },
                    fontStyle = FontStyle.Bold
                };
                GUI.Label(r, Short(bt), labelStyle);

                var start = playerProp.FindPropertyRelative("playerStartCell").vector2IntValue;
                var target = playerProp.FindPropertyRelative("targetCell").vector2IntValue;

                if (start.x == x && start.y == y) DrawCornerTag(r, "S");
                if (target.x == x && target.y == y) DrawCornerTag(r, "T", rightCorner: true);

                HandleCellInput(e, r, playerProp, cellsProp, x, y);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (e.type == EventType.MouseUp)
            isPainting = false;

        EditorGUILayout.EndVertical();
    }

    private void HandleCellInput(Event e, Rect r, SerializedProperty playerProp, SerializedProperty cellsProp, int x, int y)
    {
        if (!r.Contains(e.mousePosition))
            return;

        int w = Mathf.Max(1, playerProp.FindPropertyRelative("gridSize").vector2IntValue.x);
        int index = y * w + x;

        // Alt pick
        if (e.type == EventType.MouseDown && e.button == 0 && e.alt)
        {
            paintType = (BlockType)cellsProp.GetArrayElementAtIndex(index).enumValueIndex;
            e.Use();
            Repaint();
            return;
        }

        // Paint
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            isPainting = true;
            PaintCell(cellsProp, index);
            e.Use();
            return;
        }

        if (e.type == EventType.MouseDrag && e.button == 0 && isPainting)
        {
            PaintCell(cellsProp, index);
            e.Use();
            return;
        }

        // Context menu
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Set Player Start (S)"), false, () =>
            {
                Undo.RecordObject(level, "Set Player Start");
                playerProp.FindPropertyRelative("playerStartCell").vector2IntValue = new Vector2Int(x, y);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(level);
                Repaint();
            });

            menu.AddItem(new GUIContent("Set Target (T)"), false, () =>
            {
                Undo.RecordObject(level, "Set Target");
                playerProp.FindPropertyRelative("targetCell").vector2IntValue = new Vector2Int(x, y);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(level);
                Repaint();
            });

            menu.ShowAsContext();
            e.Use();
        }
    }

    private void PaintCell(SerializedProperty cellsProp, int index)
    {
        if (index < 0 || index >= cellsProp.arraySize) return;

        Undo.RecordObject(level, "Paint Cell");

        var el = cellsProp.GetArrayElementAtIndex(index);
        el.enumValueIndex = (int)paintType;

        so.ApplyModifiedProperties();   // important
        so.Update();
        EditorUtility.SetDirty(level);
        Repaint();
    }

    private void DrawBottomActions(SerializedProperty playerProp)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        if (GUILayout.Button("Clear Grid"))
        {
            if (EditorUtility.DisplayDialog("Clear Grid", "Set all cells to Floor?", "Yes", "Cancel"))
            {
                var size = playerProp.FindPropertyRelative("gridSize").vector2IntValue;
                var cellsProp = playerProp.FindPropertyRelative("cells");

                int w = Mathf.Max(1, size.x);
                int h = Mathf.Max(1, size.y);
                int required = w * h;

                if (cellsProp.arraySize != required)
                    EnsureCellsArraySize(playerProp, cellsProp, size);

                Undo.RecordObject(level, "Clear Grid");

                for (int i = 0; i < cellsProp.arraySize; i++)
                    cellsProp.GetArrayElementAtIndex(i).enumValueIndex = (int)BlockType.Floor;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(level);
            }
        }

        if (GUILayout.Button("Swap Left/Right"))
        {
            if (EditorUtility.DisplayDialog("Swap Left/Right", "Swap both player definitions?", "Swap", "Cancel"))
            {
                Undo.RecordObject(level, "Swap Players");
                (level.leftPlayer, level.rightPlayer) = (level.rightPlayer, level.leftPlayer);
                EditorUtility.SetDirty(level);
            }
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save"))
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorGUILayout.EndHorizontal();
    }

    private static string Short(BlockType t)
    {
        return t switch
        {
            BlockType.Empty => "EMP",
            BlockType.Floor => "FLR",
            BlockType.Trap => "TRP",
            BlockType.Obstacle => "OBS",
            BlockType.Goal => "GOL",
            BlockType.Start => "STA",
            BlockType.Bridge => "BRG",
            BlockType.ArrowDispenser => "ARR",
            BlockType.NatureBridge => "NBR",
            BlockType.Floor1 => "F1",
            _ => t.ToString().Substring(0, Mathf.Min(3, t.ToString().Length)).ToUpperInvariant()
        };
    }

    private static Color GetCellColor(BlockType t)
    {
        return t switch
        {
            BlockType.Empty => new Color(0.25f, 0.25f, 0.25f),
            BlockType.Floor => new Color(0.75f, 0.75f, 0.75f),
            BlockType.Floor1 => new Color(0.80f, 0.85f, 0.90f),
            BlockType.Trap => new Color(0.95f, 0.55f, 0.55f),
            BlockType.Obstacle => new Color(0.95f, 0.75f, 0.45f),
            BlockType.Goal => new Color(0.55f, 0.95f, 0.65f),
            BlockType.Start => new Color(0.55f, 0.75f, 0.95f),
            BlockType.Bridge => new Color(0.70f, 0.60f, 0.45f),
            BlockType.NatureBridge => new Color(0.55f, 0.95f, 0.75f),
            BlockType.ArrowDispenser => new Color(0.95f, 0.95f, 0.55f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };
    }

    private static void DrawCornerTag(Rect r, string text, bool rightCorner = false)
    {
        var tag = new Rect(
            rightCorner ? r.xMax - 14 : r.x + 2,
            r.y + 2,
            12, 12);

        var prev = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(tag, Texture2D.whiteTexture);
        GUI.color = prev;

        var style = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.black },
            fontStyle = FontStyle.Bold
        };

        GUI.Label(tag, text, style);
    }
}
#endif
