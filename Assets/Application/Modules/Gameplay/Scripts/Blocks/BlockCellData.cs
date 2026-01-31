using System;
using UnityEngine;

[Serializable]
public class BlockCellData
{
    public BlockType blockType = BlockType.Floor;
    public BlockState startState = BlockState.Active;

    [Header("Optional: per mask override (what state this block becomes when that mask is active)")]
    public MaskStateOverride[] maskOverrides;

    public BlockState GetStateForMask(MaskType mask, BlockState fallback)
    {
        if (maskOverrides == null || maskOverrides.Length == 0)
            return fallback;

        for (int i = 0; i < maskOverrides.Length; i++)
        {
            if (maskOverrides[i].mask == mask)
                return maskOverrides[i].targetState;
        }

        return fallback;
    }
}