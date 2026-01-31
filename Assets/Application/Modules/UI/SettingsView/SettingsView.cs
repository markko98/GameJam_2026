using System.Collections.Generic;
using UnityEngine;

public class SettingsView : UIViewController
{
    private SettingsViewOutlet outlet;

    public SettingsView(Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "SettingsView");
        view = Object.Instantiate(prefab, viewport, false);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponentInChildren<SettingsViewOutlet>();
        
    }

    public override void ViewWillAppear()
    {
        outlet.sfxSlider.SetValueWithoutNotify(ServiceProvider.audioService.GetMixerValue(SoundMixerType.SFX));
        outlet.musicSlider.SetValueWithoutNotify(ServiceProvider.audioService.GetMixerValue(SoundMixerType.Music));
        outlet.sfxSlider.onValueChanged.AddListener(OnSFXChange);
        outlet.musicSlider.onValueChanged.AddListener(OnMusicChange);
        outlet.closeButton.button.onClick.AddListener(ExitSettings);
    }

    private void ExitSettings()
    {
        RemoveView(0f, null, AnimationType.SlideOutRight);
    }

    private void OnSFXChange(float value)
    {
        ServiceProvider.audioService.SetMixerValue(SoundMixerType.SFX, value);
    }

    private void OnMusicChange(float value)
    {
        ServiceProvider.audioService.SetMixerValue(SoundMixerType.Music, value);
    }

    public override void ViewWillDisappear()
    {
        base.ViewWillDisappear();
        Cleanup();
    }

    public override void Cleanup()
    {
        base.Cleanup();
        outlet.sfxSlider.onValueChanged.RemoveListener(OnSFXChange);
        outlet.musicSlider.onValueChanged.RemoveListener(OnMusicChange);
        outlet.closeButton.button.onClick.RemoveListener(ExitSettings);
    }
}
