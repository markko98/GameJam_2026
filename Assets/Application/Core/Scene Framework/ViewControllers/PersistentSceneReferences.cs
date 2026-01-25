using DG.Tweening;
using UnityEngine;

/// <summary>
/// Place in entry point scene and reference needed stuff
/// </summary>

public class PersistentReferences : MonoBehaviour
{
    public Transform fullScreenViewport;
    public Transform modalViewport;
    public Transform tutorialModalViewport;
    
    private Tween bgFadeTween;
    
    private static PersistentReferences _instance;

    public static PersistentReferences Instance 
    { 
        get { return _instance; } 
    } 

    private void Awake() 
    { 
        if (_instance != null && _instance != this) 
        { 
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public Camera GetMainCamera()
    {
        return Camera.main;
    }
}