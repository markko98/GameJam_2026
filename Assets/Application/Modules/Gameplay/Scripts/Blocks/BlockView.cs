using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockView : MonoBehaviour
{
    [Serializable]
    private class BlockStateData
    {
        public MaskType mask;
        public GameObject blockVisual;
    }

    public PlayerSide Side;
    public Vector2Int Cell;
    public BlockType Type;
    public BlockState State;

    public bool isDeadly;
    
    [SerializeField] private List<BlockStateData> stateDatas = new ();
    private Renderer rend;

    public void Initialize(PlayerSide side, Vector2Int cell, BlockType type, BlockState startState)
    {
        Side = side;
        Cell = cell;
        Type = type;
        
        ApplyState(startState);
        ChangeVisuals(MaskType.None);
    }

    public void ApplyState(BlockState newState)
    {
        State = newState;

        switch (State)
        {
            case BlockState.Active:
                break;

            case BlockState.Inactive:
                break;

            case BlockState.Deadly:
                break;
            
            case BlockState.Life:
                break;
        }
    }

    public void ChangeVisuals(MaskType maskType)
    {
        foreach (var stateData in stateDatas)
        {
            stateData.blockVisual.gameObject.SetActive(false);
        }

        var dataToActivate = stateDatas.Find(x => x.mask == maskType);
        if (dataToActivate != null)
        {
            dataToActivate.blockVisual.gameObject.SetActive(true);
        }
    }
}