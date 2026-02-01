using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MaskInteractionViewOutlet: MonoBehaviour
{
    public InputActionAsset inputAsset;
    public Transform player1ControlsContainer;
    public Transform maskViewContainer;
    public Transform player2ControlsContainer;

    public List<MaskViewData> masksData = new ();

    public Image loadingBarFill;
    public Transform loadingBar;
    public Image cooldownImage;


    [Serializable]
    public class MaskViewData
    {
        public MaskType MaskType;
        public MaskViewOutlet MaskViewOutlet;
    }
}