using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;
using Object = UnityEngine.Object;
using Sequence = DG.Tweening.Sequence;

public class UIViewController
{
    #region Fields and Properties
    
    private GameObject _view;
    public GameObject view
    {
        get => _view;
        set
        {
            if (_view == value) return;
            _view = value;
            if (_view != null)
            {
                _view.SetActive(false);
                InitializeUIElements();
            }
        }
    }

    private bool isActive;
    public bool IsActive => isActive && view != null && view.activeInHierarchy;

    public AnimationType animationType = AnimationType.ScaleUpFromMiddle;
    
    protected UIStackNavigationController navigationController;

    // Cached component lookups
    private List<Button> buttons;
    private List<ButtonHandler> buttonHandlers;
    private Dictionary<string, Button> buttonCache;
    private Dictionary<string, TextMeshProUGUI> textCache;
    private Dictionary<string, Text> legacyTextCache;
    private Dictionary<string, UIViewComponent> viewCache;
    private Dictionary<string, UIImageViewComponent> imageViewCache;
    
    private TextMeshProUGUI[] texts;
    private Text[] legacyTexts;
    private UIViewComponent[] subviews;
    private UIImageViewComponent[] imageSubviews;

    // Background blur
    private Image backgroundBlurImage;
    private Object backgroundBlurImageObj;
    private Sequence backgroundBlurImageSequence;
    
    // Events
    public event Action OnViewDidLoad;
    public event Action OnViewWillAppear;
    public event Action OnViewDidAppear;
    public event Action OnViewWillDisappear;
    public event Action OnViewDidDisappear;

    #endregion

    #region Constructors

    public UIViewController()
    {
        InitializeCollections();
    }

    public UIViewController(UIStackNavigationController controller) : this()
    {
        navigationController = controller;
    }

    public UIViewController(GameObject view, UIStackNavigationController controller) : this(controller)
    {
        this.view = view;
    }

    private void InitializeCollections()
    {
        buttons = new List<Button>();
        buttonHandlers = new List<ButtonHandler>();
        buttonCache = new Dictionary<string, Button>();
        textCache = new Dictionary<string, TextMeshProUGUI>();
        legacyTextCache = new Dictionary<string, Text>();
        viewCache = new Dictionary<string, UIViewComponent>();
        imageViewCache = new Dictionary<string, UIImageViewComponent>();
    }

    #endregion

    #region Lifecycle Methods

    public virtual void ViewDidLoad()
    {
        OnViewDidLoad?.Invoke();
    }

    public virtual void ViewWillAppear()
    {
        OnViewWillAppear?.Invoke();
        isActive = true;
    }

    public virtual void ViewDidAppear()
    {
        SetButtonsInteractable(true);
        OnViewDidAppear?.Invoke();
        isActive = true;
    }

    public virtual void ViewDidDisappear()
    {
        OnViewDidDisappear?.Invoke();
        isActive = false;
    }

    public virtual void ViewWillDisappear()
    {
        SetButtonsInteractable(false);
        OnViewWillDisappear?.Invoke();
        isActive = false;
    }

    #endregion

    #region View Presentation Methods

    public virtual void PresentView(float delay = 0, AnimationType animationType = AnimationType.Default)
    {
        navigationController.Push(this, delay, animationType == AnimationType.Default ? this.animationType : animationType);
    }

    public virtual async Task PresentViewAsync(float delay = 0)
    {
        if (delay > 0)
        {
            await Task.Delay((int)(delay * 1000));
        }
        navigationController.Push(this, 0, animationType);
    }

    public virtual void RemoveView(float delay = 0, Action dismissCallback = null, AnimationType animationType = AnimationType.ScaleDownFromMiddle)
    {
        navigationController.PopController(delay, animationType);
        dismissCallback?.Invoke();
    }

    public virtual void PresentViewAsPopup(bool showBackgroundBlurImage = true, float delay = 0, AnimationType animationType = AnimationType.ScaleUpFromMiddle)
    {
        navigationController.PresentPopup(this, delay, animationType);
        if (showBackgroundBlurImage)
        {
            ShowBackgroundBlurImage();
        }
    }

    public virtual void RemovePopUp(float delay = 0, Action onComplete = null, AnimationType animationType = AnimationType.ScaleDownFromMiddle)
    {
        navigationController.RemovePopup(this, delay, onComplete, animationType);
        HideBackgroundBlurImage();
    }

    #endregion

    #region UI Initialization

    protected void InitializeUIElements()
    {
        if (_view == null) return;

        // Get all components at once
        var buttonArray = _view.GetComponentsInChildren<Button>(false);
        texts = _view.GetComponentsInChildren<TextMeshProUGUI>(true);
        legacyTexts = _view.GetComponentsInChildren<Text>(true);
        subviews = _view.GetComponentsInChildren<UIViewComponent>(true);
        imageSubviews = _view.GetComponentsInChildren<UIImageViewComponent>(true);

        backgroundBlurImageObj = Resources.Load("UI/BackgroundBlurImage");
        
        // Build button list and cache
        buttons.Clear();
        buttonCache.Clear();
        for (int i = 0; i < buttonArray.Length; i++)
        {
            var button = buttonArray[i];
            buttons.Add(button);
            buttonCache[button.name] = button;
        }

        // Initialize button handlers
        InitializeButtonHandlers();

        // Build lookup caches
        BuildTextCache();
        BuildViewCaches();

        // Call lifecycle method
        ViewDidLoad();
    }

    private void InitializeButtonHandlers()
    {
        buttonHandlers.Clear();

        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            var index = i; // Capture for closure
            var handler = new ButtonHandler(button, () =>
            {
                Debug.Log($"Button named: {button.name}, index: {index}");
            });

            buttonHandlers.Add(handler);

            button.onClick.AddListener(() =>
            {
                handler.block?.Invoke();
                // HapticManager.PlayIntervalVibration(VibrationInterval.ButtonClicked);
            });
        }
    }

    private void BuildTextCache()
    {
        textCache.Clear();
        legacyTextCache.Clear();

        for (int i = 0; i < texts.Length; i++)
        {
            var text = texts[i];
            if (!textCache.ContainsKey(text.name))
            {
                textCache[text.name] = text;
            }
        }

        for (int i = 0; i < legacyTexts.Length; i++)
        {
            var text = legacyTexts[i];
            if (!legacyTextCache.ContainsKey(text.name))
            {
                legacyTextCache[text.name] = text;
            }
        }
    }

    private void BuildViewCaches()
    {
        viewCache.Clear();
        imageViewCache.Clear();

        for (int i = 0; i < subviews.Length; i++)
        {
            var subview = subviews[i];
            if (!viewCache.ContainsKey(subview.name))
            {
                viewCache[subview.name] = subview;
            }
        }

        for (int i = 0; i < imageSubviews.Length; i++)
        {
            var imageView = imageSubviews[i];
            if (!imageViewCache.ContainsKey(imageView.name))
            {
                imageViewCache[imageView.name] = imageView;
            }
        }
    }

    public virtual void InitializeWithView(GameObject view)
    {
        this.view = view;
    }

    #endregion

    #region Button Management

    public void SetButtonsInteractable(bool interactable)
    {
        int count = buttons.Count;
        for (int i = 0; i < count; i++)
        {
            buttons[i].interactable = interactable;
        }
    }

    public virtual void RegisterPressedButtonWithIndex(int index, Action block)
    {
        if (index >= 0 && index < buttonHandlers.Count)
        {
            buttonHandlers[index].block = block;
        }
        else
        {
            Debug.LogWarning($"Button index {index} out of range");
        }
    }

    public virtual void RegisterPressedButtonWithName(string name, Action block)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Button name is null or empty");
            return;
        }

        bool didFindButton = false;
        for (int i = 0; i < buttonHandlers.Count; i++)
        {
            if (buttonHandlers[i].button.name == name)
            {
                buttonHandlers[i].block = block;
                didFindButton = true;
                break;
            }
        }

        if (!didFindButton)
        {
            Debug.LogWarning($"Couldn't find button with name: {name}");
        }
    }

    public virtual Button GetButtonWithName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        
        return buttonCache.TryGetValue(name, out var button) ? button : null;
    }

    #endregion

    #region Component Retrieval

    public TextMeshProUGUI GetTextWithName(string textName)
    {
        if (string.IsNullOrEmpty(textName)) return null;

        if (textCache.TryGetValue(textName, out var text))
        {
            return text;
        }

        Debug.LogError($"Couldn't find Text named: {textName}");
        return null;
    }

    public Text GetLegacyTextWithName(string textName)
    {
        if (string.IsNullOrEmpty(textName)) return null;

        if (legacyTextCache.TryGetValue(textName, out var text))
        {
            return text;
        }

        Debug.LogError($"Couldn't find Text named: {textName}");
        return null;
    }

    public UIViewComponent GetUIViewComponentWithName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        if (viewCache.TryGetValue(name, out var view))
        {
            return view;
        }

        Debug.LogError($"Couldn't find UIViewComponent named: {name}");
        return null;
    }

    public UIImageViewComponent GetUIImageViewComponentWithName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        if (imageViewCache.TryGetValue(name, out var imageView))
        {
            return imageView;
        }

        Debug.LogError($"Couldn't find UIImageViewComponent named: {name}");
        return null;
    }

    #endregion

    #region Background Blur

    public void ShowBackgroundBlurImage()
    {
        backgroundBlurImageSequence?.Kill(true);
        
        backgroundBlurImage = Object.Instantiate(backgroundBlurImageObj, PersistentReferences.Instance.fullScreenViewport).GetComponent<Image>();
        if (backgroundBlurImage == null) return;
        
        backgroundBlurImage.transform.SetAsFirstSibling();
        backgroundBlurImageSequence = DOTween.Sequence();
        backgroundBlurImageSequence.Append(backgroundBlurImage.DOFade(0, 0));
        backgroundBlurImageSequence.Append(backgroundBlurImage.DOFade(0.9f, 0.2f));
    }

    public void HideBackgroundBlurImage()
    {
        if (backgroundBlurImage == null) return;

        backgroundBlurImageSequence?.Kill();
        backgroundBlurImageSequence = DOTween.Sequence();
        backgroundBlurImageSequence.Append(backgroundBlurImage.DOFade(0, 0.2f));
        backgroundBlurImageSequence.OnComplete(() =>
        {
            Object.Destroy(backgroundBlurImage.gameObject);
            backgroundBlurImage = null;
        });
    }

    #endregion

    #region Cleanup

    public virtual void Cleanup()
    {
        // Kill animations
        backgroundBlurImageSequence?.Kill();
        backgroundBlurImageSequence = null;

        // Clear button listeners
        if (buttons != null)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].onClick.RemoveAllListeners();
                }
            }
        }

        // Clear collections
        buttonHandlers?.Clear();
        buttonCache?.Clear();
        textCache?.Clear();
        legacyTextCache?.Clear();
        viewCache?.Clear();
        imageViewCache?.Clear();

        // Clear events
        OnViewDidLoad = null;
        OnViewWillAppear = null;
        OnViewDidAppear = null;
        OnViewWillDisappear = null;
        OnViewDidDisappear = null;
    }

    #endregion

    #region Nested Classes

    public class ButtonHandler
    {
        public Button button;
        public Action block;

        public ButtonHandler(Button button, Action block)
        {
            this.button = button;
            this.block = block;
        }
    }

    #endregion
}