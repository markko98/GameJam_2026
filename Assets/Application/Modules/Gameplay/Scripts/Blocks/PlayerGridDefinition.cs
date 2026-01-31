using System;
using UnityEngine;

[Serializable]
public class PlayerGridDefinition
{
    [Header("Layout")]
    public Vector2Int gridSize = new Vector2Int(5, 5);
    public float cellSize = 1f;

    [Header("Player/Goal")]
    public Vector2Int playerStartCell = new Vector2Int(0, 0);
    public Vector2Int targetCell = new Vector2Int(4, 4);

    [Header("Masks available in this player's level (optional)")]
    public MaskType[] availableMasks;

    [Header("Cells (size must be width*height)")]
    public BlockCellData[] cells;

    public bool IsInside(Vector2Int c)
    {
        return c.x >= 0 && c.y >= 0 && c.x < gridSize.x && c.y < gridSize.y;
    }

    public BlockCellData GetCell(int x, int y)
    {
        if (cells == null) return null;
        int w = gridSize.x;
        int h = gridSize.y;
        if (x < 0 || y < 0 || x >= w || y >= h) return null;

        int index = y * w + x;
        if (index < 0 || index >= cells.Length) return null;

        return cells[index];
    }

#if UNITY_EDITOR
    public void ValidateOrFix()
    {
        int required = Mathf.Max(1, gridSize.x * gridSize.y);
        if (cells == null || cells.Length != required)
        {
            var newArr = new BlockCellData[required];
            if (cells != null)
            {
                int copy = Mathf.Min(cells.Length, required);
                Array.Copy(cells, newArr, copy);
            }

            for (int i = 0; i < required; i++)
            {
                if (newArr[i] == null)
                    newArr[i] = new BlockCellData();
            }

            cells = newArr;
        }
    }
#endif
}