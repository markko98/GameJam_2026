#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class LevelSODefinitionWindow : EditorWindow
{
    private LevelSO level;
    private SerializedObject so;

    // UI state
    private PlayerSide editingSide = PlayerSide.Left;

    private BlockType paintType = BlockType.Floor;
    private BlockState paintState = BlockState.Active;

    private enum PaintMode { TypeOnly, StateOnly, TypeAndState }
    private PaintMode paintMode = PaintMode.TypeAndState;

    private bool isPainting;
    private Vector2 scroll;

    // Grid visuals
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
        // If you select a LevelSO asset in Project, auto-assign it
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

        EditorGUILayout.Space(6);
        var playerProp = GetPlayerProp(editingSide);
        if (playerProp == null)
        {
            EditorGUILayout.HelpBox("Player definition is missing in LevelSO.", MessageType.Error);
            so.ApplyModifiedProperties();
            return;
        }

        DrawGridSettings(playerProp);

        EditorGUILayout.Space(8);
        DrawPalette();

        EditorGUILayout.Space(8);
        DrawGrid(playerProp);

        EditorGUILayout.Space(10);
        DrawBottomActions(playerProp);

        so.ApplyModifiedProperties();

        // Keep asset dirty if changed via GUI
        if (GUI.changed)
        {
            EditorUtility.SetDirty(level);
        }
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
        // LevelSO fields: leftPlayer / rightPlayer
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

        // Show info
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

        // Clamp size back into prop (in case user typed 0 or negative)
        var sizeProp = playerProp.FindPropertyRelative("gridSize");
        sizeProp.vector2IntValue = new Vector2Int(w, h);

        if (cellsProp.arraySize == required) return;

        Undo.RecordObject(level, "Resize Grid Cells");

        int old = cellsProp.arraySize;
        cellsProp.arraySize = required;

        // Initialize new elements if expanded
        if (required > old)
        {
            for (int i = old; i < required; i++)
            {
                var cell = cellsProp.GetArrayElementAtIndex(i);
                // cell is BlockCellData (class) => might be null
                if (cell.managedReferenceValue == null && cell.propertyType == SerializedPropertyType.ManagedReference)
                {
                    // Not expected with your current BlockCellData (Serializable class),
                    // so we do nothing here.
                }
                // For Serializable classes in arrays, Unity typically creates default instances.
                // We'll still try to set defaults defensively:
                var bt = cell.FindPropertyRelative("blockType");
                var st = cell.FindPropertyRelative("startState");
                if (bt != null) bt.enumValueIndex = (int)BlockType.Floor;
                if (st != null) st.enumValueIndex = (int)BlockState.Active;
            }
        }
    }

    private void DrawPalette()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Paint Palette", EditorStyles.boldLabel);

        paintMode = (PaintMode)EditorGUILayout.EnumPopup("Mode", paintMode);
        paintType = (BlockType)EditorGUILayout.EnumPopup("Block Type", paintType);
        paintState = (BlockState)EditorGUILayout.EnumPopup("Block State", paintState);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Pick From Cell (Alt+Click)"))
        {
            EditorGUILayout.HelpBox("Hold Alt and click a cell to pick its type/state into palette.", MessageType.None);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Stop Painting", GUILayout.Width(110)))
            isPainting = false;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "Left Click = paint. Drag = paint multiple. Alt+Click = pick from cell.\n" +
            "Tip: Use Mode to paint only type or only state.",
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

        // grid scroll if big
        scroll = EditorGUILayout.BeginScrollView(scroll);

        // Detect mouse events for drag painting
        var e = Event.current;

        // Draw rows from top to bottom (y=h-1..0) so it feels natural
        for (int y = h - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(6);

            for (int x = 0; x < w; x++)
            {
                int index = y * w + x;
                var cellProp = cellsProp.GetArrayElementAtIndex(index);

                var btProp = cellProp.FindPropertyRelative("blockType");
                var stProp = cellProp.FindPropertyRelative("startState");

                BlockType bt = (BlockType)(btProp?.enumValueIndex ?? 0);
                BlockState st = (BlockState)(stProp?.enumValueIndex ?? 0);

                Rect r = GUILayoutUtility.GetRect(cellPx, cellPx, GUILayout.Width(cellPx), GUILayout.Height(cellPx));
                r = new Rect(r.x + cellPad * 0.5f, r.y + cellPad * 0.5f, r.width - cellPad, r.height - cellPad);

                // background color based on type/state
                Color prev = GUI.color;
                GUI.color = GetCellColor(bt, st);

                GUI.Box(r, GUIContent.none);

                GUI.color = prev;

                // Labels
                var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.black },
                    fontStyle = FontStyle.Bold
                };

                GUI.Label(r, $"{Short(bt)}\n{Short(st)}", labelStyle);

                // Start/Target markers
                var start = playerProp.FindPropertyRelative("playerStartCell").vector2IntValue;
                var target = playerProp.FindPropertyRelative("targetCell").vector2IntValue;

                if (start.x == x && start.y == y)
                    DrawCornerTag(r, "S");

                if (target.x == x && target.y == y)
                    DrawCornerTag(r, "T", rightCorner: true);

                // Handle interactions
                HandleCellInput(e, r, playerProp, cellsProp, x, y);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // Stop painting on mouse up anywhere
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

        // Alt+Click picks
        if (e.type == EventType.MouseDown && e.button == 0 && e.alt)
        {
            var cell = cellsProp.GetArrayElementAtIndex(index);
            var bt = cell.FindPropertyRelative("blockType");
            var st = cell.FindPropertyRelative("startState");

            if (bt != null) paintType = (BlockType)bt.enumValueIndex;
            if (st != null) paintState = (BlockState)st.enumValueIndex;

            e.Use();
            Repaint();
            return;
        }

        // Left click starts painting
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            isPainting = true;
            PaintCell(cellsProp, index);
            e.Use();
            return;
        }

        // Drag paint
        if (e.type == EventType.MouseDrag && e.button == 0 && isPainting)
        {
            PaintCell(cellsProp, index);
            e.Use();
            return;
        }

        // Right click context: set Start/Target quickly
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Set Player Start (S)"), false, () =>
            {
                Undo.RecordObject(level, "Set Player Start");
                playerProp.FindPropertyRelative("playerStartCell").vector2IntValue = new Vector2Int(x, y);
                EditorUtility.SetDirty(level);
                Repaint();
            });

            menu.AddItem(new GUIContent("Set Target (T)"), false, () =>
            {
                Undo.RecordObject(level, "Set Target");
                playerProp.FindPropertyRelative("targetCell").vector2IntValue = new Vector2Int(x, y);
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

        var cell = cellsProp.GetArrayElementAtIndex(index);
        var bt = cell.FindPropertyRelative("blockType");
        var st = cell.FindPropertyRelative("startState");

        switch (paintMode)
        {
            case PaintMode.TypeOnly:
                if (bt != null) bt.enumValueIndex = (int)paintType;
                break;
            case PaintMode.StateOnly:
                if (st != null) st.enumValueIndex = (int)paintState;
                break;
            case PaintMode.TypeAndState:
                if (bt != null) bt.enumValueIndex = (int)paintType;
                if (st != null) st.enumValueIndex = (int)paintState;
                break;
        }

        EditorUtility.SetDirty(level);
        Repaint();
    }

    private void DrawBottomActions(SerializedProperty playerProp)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        if (GUILayout.Button("Clear Grid"))
        {
            if (EditorUtility.DisplayDialog("Clear Grid", "Set all cells to Floor + Active?", "Yes", "Cancel"))
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
                {
                    var cell = cellsProp.GetArrayElementAtIndex(i);
                    var bt = cell.FindPropertyRelative("blockType");
                    var st = cell.FindPropertyRelative("startState");
                    if (bt != null) bt.enumValueIndex = (int)BlockType.Floor;
                    if (st != null) st.enumValueIndex = (int)BlockState.Active;
                }

                EditorUtility.SetDirty(level);
            }
        }

        if (GUILayout.Button("Swap Left/Right"))
        {
            if (EditorUtility.DisplayDialog("Swap Left/Right", "Swap both player definitions?", "Swap", "Cancel"))
            {
                Undo.RecordObject(level, "Swap Players");
                SwapPlayers(level);
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

    private static void SwapPlayers(LevelSO lvl)
    {
        // Reflection-free swap for known fields (leftPlayer/rightPlayer)
        (lvl.leftPlayer, lvl.rightPlayer) = (lvl.rightPlayer, lvl.leftPlayer);
    }

    // Visual helpers
    private static string Short(BlockType t)
    {
        return t switch
        {
            BlockType.Empty => "EMP",
            BlockType.Floor => "FLR",
            BlockType.Trap => "TRP",
            BlockType.Obstacle => "OBS",
            BlockType.Goal => "GOL",
            _ => t.ToString().Substring(0, Mathf.Min(3, t.ToString().Length)).ToUpperInvariant()
        };
    }

    private static string Short(BlockState s)
    {
        return s switch
        {
            BlockState.Active => "ON",
            BlockState.Inactive => "OFF",
            BlockState.Deadly => "DMG",
            _ => s.ToString().Substring(0, Mathf.Min(3, s.ToString().Length)).ToUpperInvariant()
        };
    }

    private static Color GetCellColor(BlockType t, BlockState s)
    {
        // Simple readable palette (no fancy styling)
        // type influences hue, state influences brightness
        Color c = t switch
        {
            BlockType.Empty => new Color(0.25f, 0.25f, 0.25f),
            BlockType.Floor => new Color(0.75f, 0.75f, 0.75f),
            BlockType.Trap => new Color(0.95f, 0.55f, 0.55f),
            BlockType.Obstacle => new Color(0.95f, 0.75f, 0.45f),
            BlockType.Goal => new Color(0.55f, 0.95f, 0.65f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };

        float mult = s switch
        {
            BlockState.Active => 1.00f,
            BlockState.Inactive => 0.45f,
            BlockState.Deadly => 0.85f,
            _ => 1.0f
        };

        return new Color(c.r * mult, c.g * mult, c.b * mult);
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
