using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class VideoView: UIViewController
{
    private VideoViewOutlet outlet;
    private LoadingView loadingView;

    private Action closeCallback;
    
    public VideoView(Action closeCallback, Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        this.closeCallback = closeCallback;
        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "VideoView");
        view = Object.Instantiate(prefab, viewport, false);
    }
    
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponentInChildren<VideoViewOutlet>();
    }

    public override void ViewDidAppear()
    {
        base.ViewDidAppear();
        SetupVideo();
        
        outlet.skipButton.onClick.AddListener(Exit);
    }

    private void SetupVideo()
    {
        outlet.videoPlayer.Play();
        outlet.videoPlayer.loopPointReached += (e) => Exit();
    }

    private void Exit()
    {
        RemoveView(0, closeCallback);
    }

    public override void ViewWillDisappear()
    {
        base.ViewWillDisappear();
        Cleanup();
    }

    public override void Cleanup()
    {
        base.Cleanup();
        outlet.videoPlayer.Stop();
    }
}