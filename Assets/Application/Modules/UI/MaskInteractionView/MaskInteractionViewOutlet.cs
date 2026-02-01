using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class MaskInteractionViewOutlet: MonoBehaviour
{
    public InputActionAsset inputAsset;
    public Transform player1ControlsContainer;
    public Transform maskViewContainer;
    public Transform player2ControlsContainer;

    public List<MaskViewData> masksData = new ();


    [Serializable]
    public class MaskViewData
    {
        public MaskType MaskType;
        public MaskViewOutlet MaskViewOutlet;
    }
}