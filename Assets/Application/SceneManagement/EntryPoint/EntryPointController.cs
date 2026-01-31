using System;
using UnityEngine;
using Application = UnityEngine.Device.Application;
using Debug = UnityEngine.Debug;

public class EntryPointController : AppDelegate
{
    private EntryPointOutlet outlet;
    private UIStackNavigationController uiStackController;

    public override void ApplicationStarted()
    {
        base.ApplicationStarted();
        outlet = GameObject.Find(OutletNames.EntryPoint).GetComponent<EntryPointOutlet>();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        uiStackController = new UIStackNavigationController(outlet.canvas.transform);
        Load();
    }

    private async void Load()
    {
        GameTicker.Initialize();
        AudioProvider.Prewarm();
        SpriteProvider.Prewarm();
        
        InitializeServices();
        
        LoadDefaultSceneAsync();
    }

    private void InitializeServices()
    {
        ServiceProvider.audioService = new AudioService();
        ServiceProvider.modalService = new ModalService();
        ServiceProvider.currencyService = new CurrencyService();
        Debug.Log("Services initialization");
    }
    
    
    private void LoadDefaultSceneAsync()
    {
        USceneController sceneController = DetermineSceneController();
        UNavigationController.SetRootViewController(sceneController,false);
    }

    private USceneController DetermineSceneController()
    {
        return new MainMenuController();
    }
}