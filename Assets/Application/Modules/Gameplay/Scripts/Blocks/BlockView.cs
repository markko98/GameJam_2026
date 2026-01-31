using UnityEngine;

public class BlockView : MonoBehaviour
{
    public PlayerSide Side { get; private set; }
    public Vector2Int Cell { get; private set; }
    public BlockType Type { get; private set; }
    public BlockState State { get; private set; }

    // Optional visuals (e.g. disable renderer/collider)
    private Collider col;
    private Renderer rend;

    public void Initialize(PlayerSide side, Vector2Int cell, BlockType type, BlockState startState)
    {
        Side = side;
        Cell = cell;
        Type = type;

        col = GetComponent<Collider>();
        rend = GetComponentInChildren<Renderer>();

        ApplyState(startState);
    }

    public void ApplyState(BlockState newState)
    {
        State = newState;

        // Basic behavior:
        // Active: solid + visible
        // Inactive: non-solid + maybe invisible
        // Deadly: solid + visible (but special flag)
        switch (State)
        {
            case BlockState.Active:
                if (col) col.enabled = true;
                if (rend) rend.enabled = true;
                break;

            case BlockState.Inactive:
                if (col) col.enabled = false;
                if (rend) rend.enabled = false;
                break;

            case BlockState.Deadly:
                if (col) col.enabled = true;
                if (rend) rend.enabled = true;
                break;
        }
    }
}